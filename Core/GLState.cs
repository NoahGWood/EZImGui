using OpenTK.Graphics.OpenGL4;

namespace EZImGui.Core
{
    public static class GLState
    {
        static int prevVAO;
        static int prevArrayBuffer;
        static int prevProgram;
        static bool prevBlendEnabled;
        static bool prevScissorTestEnabled;
        static int prevBlendEquationRgb;
        static int prevBlendEquationAlpha;
        static int prevBlendFuncSrcRgb;
        static int prevBlendFuncSrcAlpha;
        static int prevBlendFuncDstRgb;
        static int prevBlendFuncDstAlpha;
        static bool prevCullFaceEnabled;
        static bool prevDepthTestEnabled;
        static int prevActiveTexture;
        static int prevTexture2D;

        public static void SaveState()
        {
            prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
            prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
            prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
        }

        public static void RestoreState()
        {
            // Reset state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);
            GL.UseProgram(prevProgram);
            GL.BindVertexArray(prevVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
            GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
            GL.BlendFuncSeparate(
                (BlendingFactorSrc)prevBlendFuncSrcRgb,
                (BlendingFactorDest)prevBlendFuncDstRgb,
                (BlendingFactorSrc)prevBlendFuncSrcAlpha,
                (BlendingFactorDest)prevBlendFuncDstAlpha);
            if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
        }
    }
}
