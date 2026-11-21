using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KuwaharaURP {
    [System.Serializable]
    public sealed class KuwaharaEffectPass : ScriptableRenderPass {
       RenderTargetIdentifier _source;
       RenderTargetIdentifier _destinationA;
       RenderTargetIdentifier _destinationB;
       RenderTargetIdentifier _latestDest;

       RenderTargetIdentifier _structureTensor;
       RenderTargetIdentifier _eigenvectors1;
       RenderTargetIdentifier _eigenvectors2;

       readonly int _stensorID       = Shader.PropertyToID("_StructureTensor");
       readonly int _eigenvectors1ID = Shader.PropertyToID("_Eigenvectors1");
       readonly int _eigenvectors2ID = Shader.PropertyToID("_Eigenvectors2");

       readonly int temporaryRTIdA   = Shader.PropertyToID("_TempRT");
       readonly int temporaryRTIdB   = Shader.PropertyToID("_TempRTB");

       Material           _effectMaterial;
       KuwaharaEffectType _effectType;
       bool               _baseAllocated;
       bool               _anisotropicAllocated;

       public KuwaharaEffectPass() {
          renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
       }

       [System.Obsolete]
       public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
          _baseAllocated = false;
          _anisotropicAllocated = false;

          if ( renderingData.cameraData.isSceneViewCamera ) {
             return;
          }

          var stack = VolumeManager.instance.stack;
          var customEffect = stack.GetComponent<KuwaharaEffectPPComponent>();
          if ( customEffect == null || !customEffect.IsActive() || !customEffect.Enabled.value ) {
             return;
          }

          bool isOverlay = renderingData.cameraData.renderType == CameraRenderType.Overlay;
          // Если это камера персонажей (Overlay) и галочка ApplyToOverlay выключена — пропускаем
          if ( isOverlay && !customEffect.ApplyToOverlay.value ) {
             return;
          }

          RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
          descriptor.depthBufferBits = 0;

          var renderer = renderingData.cameraData.renderer;
          _source = renderer.cameraColorTargetHandle;

          cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
          _destinationA = new RenderTargetIdentifier(temporaryRTIdA);
          cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
          _destinationB = new RenderTargetIdentifier(temporaryRTIdB);
          _baseAllocated = true;

          if ( customEffect.EffectType.value == KuwaharaEffectType.Anisotropic ) {
             cmd.GetTemporaryRT(_stensorID, descriptor, FilterMode.Bilinear);
             _structureTensor = new RenderTargetIdentifier(_stensorID);

             cmd.GetTemporaryRT(_eigenvectors1ID, descriptor, FilterMode.Bilinear);
             _eigenvectors1 = new RenderTargetIdentifier(_eigenvectors1ID);

             cmd.GetTemporaryRT(_eigenvectors2ID, descriptor, FilterMode.Bilinear);
             _eigenvectors2 = new RenderTargetIdentifier(_eigenvectors2ID);

             _anisotropicAllocated = true;
          }
       }

       [System.Obsolete]
       public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
          if ( !_baseAllocated || renderingData.cameraData.isSceneViewCamera ) {
             return;
          }

          var stack = VolumeManager.instance.stack;
          var customEffect = stack.GetComponent<KuwaharaEffectPPComponent>();
          if ( customEffect == null || !customEffect.IsActive() || !customEffect.Enabled.value ) {
             return;
          }

          bool isOverlay = renderingData.cameraData.renderType == CameraRenderType.Overlay;
          if ( isOverlay && !customEffect.ApplyToOverlay.value ) {
             return;
          }

          CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
          cmd.Clear();

          #region Local Methods
          void BlitTo(Material mat, int pass = 0) {
             var first = _latestDest;
             var last = first == _destinationA ? _destinationB : _destinationA;
             cmd.Blit(first, last, mat, pass);

             _latestDest = last;
          }

          void RenderPasses(int passCount) {
             for ( int i = 0; i < passCount; ++i ) {
                BlitTo(_effectMaterial);
             }
          }
          #endregion

          _latestDest = _source;

          // Выбираем ползунки: для Base камеры — основные, для Overlay камеры (персонажей) — новые
          int passCount  = isOverlay ? customEffect.OverlayPasses.value     : customEffect.Passes.value;
          int kernelSize = isOverlay ? customEffect.OverlayKernelSize.value : customEffect.KernelSize.value;
          float alpha    = isOverlay ? customEffect.OverlayAlpha.value      : customEffect.Alpha.value;
          float zeta     = customEffect.UseZeta.value ? customEffect.Zeta.value : 2.0f / (kernelSize / 2.0f);

          switch ( customEffect.EffectType.value ) {
             case KuwaharaEffectType.Basic:
                SetupBasic(customEffect);
                _effectMaterial.SetInt("_KernelSize", kernelSize);
                RenderPasses(passCount);
                break;

             case KuwaharaEffectType.Generalized:
                SetupGeneralized(customEffect);
                _effectMaterial.SetInt("_KernelSize", kernelSize);
                _effectMaterial.SetFloat("_Zeta", zeta);
                RenderPasses(passCount);
                break;

             case KuwaharaEffectType.Anisotropic:
                if ( !_anisotropicAllocated ) break;
                SetupAnisotropic(customEffect);
                _effectMaterial.SetInt("_KernelSize", kernelSize);
                _effectMaterial.SetFloat("_Alpha", alpha);
                _effectMaterial.SetFloat("_Zeta", zeta);

                cmd.Blit(_source, _structureTensor, _effectMaterial, 0);
                cmd.Blit(_structureTensor, _eigenvectors1, _effectMaterial, 1);
                cmd.Blit(_structureTensor, _eigenvectors2, _effectMaterial, 2);
                cmd.SetGlobalTexture("_TFM", _eigenvectors2);
                for ( int i = 0; i < passCount; ++i ) {
                   BlitTo(_effectMaterial, 3);
                }
                break;
          }

          cmd.Blit(_latestDest, _source);

          context.ExecuteCommandBuffer(cmd);
          CommandBufferPool.Release(cmd);
       }

       void SetupBasic(KuwaharaEffectPPComponent ppComponent) {
          if ( _effectType != KuwaharaEffectType.Basic || !_effectMaterial ) {
             if ( _effectMaterial ) {
                Object.Destroy(_effectMaterial);
             }
             _effectMaterial = new Material(Resources.Load<Shader>("Shaders/Kuwahara"));
             _effectType = ppComponent.EffectType.value;
          }

          _effectMaterial.SetInt("_KernelSize", ppComponent.KernelSize.value);
          _effectMaterial.SetInt("_MinKernelSize", ppComponent.MinKernelSize.value);
          _effectMaterial.SetInt("_AnimateSize", ppComponent.AnimateKernelSize.value ? 1 : 0);
          _effectMaterial.SetFloat("_SizeAnimationSpeed", ppComponent.SizeAnimationSpeed.value);
          _effectMaterial.SetFloat("_NoiseFrequency", ppComponent.NoiseFrequency.value);
          _effectMaterial.SetInt("_AnimateOrigin", ppComponent.AnimateKernelOrigin.value ? 1 : 0);
       }

       void SetupGeneralized(KuwaharaEffectPPComponent ppComponent) {
          if ( _effectType != KuwaharaEffectType.Generalized || !_effectMaterial ) {
             if ( _effectMaterial ) {
                Object.Destroy(_effectMaterial);
             }
             _effectMaterial = new Material(Resources.Load<Shader>("Shaders/GeneralizedKuwahara"));
             _effectType = ppComponent.EffectType.value;
          }
          _effectMaterial.SetInt("_KernelSize", ppComponent.KernelSize.value);
          _effectMaterial.SetInt("_N", 8);
          _effectMaterial.SetFloat("_Q", ppComponent.Sharpness.value);
          _effectMaterial.SetFloat("_Hardness", ppComponent.Hardness.value);
          _effectMaterial.SetFloat("_ZeroCrossing", ppComponent.ZeroCrossing.value);
          _effectMaterial.SetFloat("_Zeta", ppComponent.UseZeta.value ? ppComponent.Zeta.value : 2.0f / (ppComponent.KernelSize.value / 2.0f));
       }

       void SetupAnisotropic(KuwaharaEffectPPComponent ppComponent) {
          if ( _effectType != KuwaharaEffectType.Anisotropic || !_effectMaterial ) {
             if ( _effectMaterial ) {
                Object.Destroy(_effectMaterial);
             }
             _effectMaterial = new Material(Resources.Load<Shader>("Shaders/AnisotropicKuwahara"));
             _effectType = ppComponent.EffectType.value;
          }
          _effectMaterial.SetInt("_KernelSize", ppComponent.KernelSize.value);
          _effectMaterial.SetInt("_N", 8);
          _effectMaterial.SetFloat("_Q", ppComponent.Sharpness.value);
          _effectMaterial.SetFloat("_Hardness", ppComponent.Hardness.value);
          _effectMaterial.SetFloat("_ZeroCrossing", ppComponent.ZeroCrossing.value);
          _effectMaterial.SetFloat("_Alpha", ppComponent.Alpha.value);
          _effectMaterial.SetFloat("_Zeta", ppComponent.UseZeta.value ? ppComponent.Zeta.value : 2.0f / (ppComponent.KernelSize.value / 2.0f));
       }

       public override void OnCameraCleanup(CommandBuffer cmd) {
          if ( _baseAllocated ) {
             cmd.ReleaseTemporaryRT(temporaryRTIdA);
             cmd.ReleaseTemporaryRT(temporaryRTIdB);
             _baseAllocated = false;
          }

          if ( _anisotropicAllocated ) {
             cmd.ReleaseTemporaryRT(_stensorID);
             cmd.ReleaseTemporaryRT(_eigenvectors1ID);
             cmd.ReleaseTemporaryRT(_eigenvectors2ID);
             _anisotropicAllocated = false;
          }
       }

       public void DeInit() {
          if ( _effectMaterial ) {
             Object.Destroy( _effectMaterial );
             _effectMaterial = null;
          }
       }
    }
}