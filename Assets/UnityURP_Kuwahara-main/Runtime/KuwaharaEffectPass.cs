using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace KuwaharaURP {
    [System.Serializable]
    public sealed class KuwaharaEffectPass : ScriptableRenderPass {
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

       public KuwaharaEffectPass() {
          renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
       }

#if UNITY_6000_0_OR_NEWER
       private class PassData {
          public TextureHandle source;
          public RenderTextureDescriptor descriptor;
          public bool isOverlay;
          public KuwaharaEffectPass pass;
       }

       public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
          var cameraData = frameData.Get<UniversalCameraData>();
          if ( cameraData.isSceneViewCamera ) {
             return;
          }

          var resourceData = frameData.Get<UniversalResourceData>();
          if ( resourceData.isActiveTargetBackBuffer ) {
             return;
          }

          var stack = VolumeManager.instance.stack;
          var customEffect = stack.GetComponent<KuwaharaEffectPPComponent>();
          if ( customEffect == null || !customEffect.IsActive() || !customEffect.Enabled.value ) {
             return;
          }

          bool isOverlay = cameraData.renderType == CameraRenderType.Overlay;
          if ( isOverlay && !customEffect.ApplyToOverlay.value ) {
             return;
          }

          using ( var builder = renderGraph.AddUnsafePass<PassData>("Kuwahara Effect", out var passData) ) {
             passData.source = resourceData.activeColorTexture;
             passData.descriptor = cameraData.cameraTargetDescriptor;
             passData.isOverlay = isOverlay;
             passData.pass = this;

             builder.UseTexture(passData.source, AccessFlags.ReadWrite);
             builder.AllowPassCulling(false);

             builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => {
                var currentStack = VolumeManager.instance.stack;
                var effect = currentStack.GetComponent<KuwaharaEffectPPComponent>();
                if ( effect == null ) return;

                CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                data.pass.RenderKuwahara(cmd, data.source, data.descriptor, effect, data.isOverlay);
             });
          }
       }
#endif

#if URP_COMPATIBILITY_MODE || !UNITY_6000_0_OR_NEWER
       [System.Obsolete]
       public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
          if ( renderingData.cameraData.isSceneViewCamera ) {
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

          var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
          var descriptor = renderingData.cameraData.cameraTargetDescriptor;

          RenderKuwahara(cmd, source, descriptor, customEffect, isOverlay);

          context.ExecuteCommandBuffer(cmd);
          CommandBufferPool.Release(cmd);
       }
#endif

       void RenderKuwahara(CommandBuffer cmd, RenderTargetIdentifier source, RenderTextureDescriptor descriptor, KuwaharaEffectPPComponent customEffect, bool isOverlay) {
          descriptor.depthBufferBits = 0;

          cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
          _destinationA = new RenderTargetIdentifier(temporaryRTIdA);
          cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
          _destinationB = new RenderTargetIdentifier(temporaryRTIdB);

          bool isAnisotropic = customEffect.EffectType.value == KuwaharaEffectType.Anisotropic;
          if ( isAnisotropic ) {
             cmd.GetTemporaryRT(_stensorID, descriptor, FilterMode.Bilinear);
             _structureTensor = new RenderTargetIdentifier(_stensorID);

             cmd.GetTemporaryRT(_eigenvectors1ID, descriptor, FilterMode.Bilinear);
             _eigenvectors1 = new RenderTargetIdentifier(_eigenvectors1ID);

             cmd.GetTemporaryRT(_eigenvectors2ID, descriptor, FilterMode.Bilinear);
             _eigenvectors2 = new RenderTargetIdentifier(_eigenvectors2ID);
          }

          #region Local Methods
          void BlitTo(Material mat, int pass = 0) {
             var first = _latestDest;
             var last = first == _destinationA ? _destinationB : _destinationA;
             cmd.Blit(first, last, mat, pass);

             _latestDest = last;
          }

          void RenderPasses(int count) {
             for ( int i = 0; i < count; ++i ) {
                BlitTo(_effectMaterial);
             }
          }
          #endregion

          _latestDest = source;

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
                SetupAnisotropic(customEffect);
                _effectMaterial.SetInt("_KernelSize", kernelSize);
                _effectMaterial.SetFloat("_Alpha", alpha);
                _effectMaterial.SetFloat("_Zeta", zeta);

                cmd.Blit(source, _structureTensor, _effectMaterial, 0);
                cmd.Blit(_structureTensor, _eigenvectors1, _effectMaterial, 1);
                cmd.Blit(_eigenvectors1, _eigenvectors2, _effectMaterial, 2);
                cmd.SetGlobalTexture("_TFM", _eigenvectors2);
                for ( int i = 0; i < passCount; ++i ) {
                   BlitTo(_effectMaterial, 3);
                }
                break;
          }

          cmd.Blit(_latestDest, source);

          cmd.ReleaseTemporaryRT(temporaryRTIdA);
          cmd.ReleaseTemporaryRT(temporaryRTIdB);
          if ( isAnisotropic ) {
             cmd.ReleaseTemporaryRT(_stensorID);
             cmd.ReleaseTemporaryRT(_eigenvectors1ID);
             cmd.ReleaseTemporaryRT(_eigenvectors2ID);
          }
       }

       void SetupBasic(KuwaharaEffectPPComponent ppComponent) {
          if ( _effectType != KuwaharaEffectType.Basic || !_effectMaterial ) {
             if ( _effectMaterial ) {
                CoreUtils.Destroy(_effectMaterial);
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
                CoreUtils.Destroy(_effectMaterial);
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
                CoreUtils.Destroy(_effectMaterial);
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

       public void DeInit() {
          if ( _effectMaterial ) {
             CoreUtils.Destroy( _effectMaterial );
             _effectMaterial = null;
          }
       }
    }
}