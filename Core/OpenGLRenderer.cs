using OpenTK.Graphics.OpenGL4;
using System;

namespace EZImGui.Core
{
    public static class OpenGLRenderer
    {
        public static bool KHRDebugAvailable { get; private set; }

        public static readonly int GLVersion;
        public static readonly bool CompatibilityProfile;

        static OpenGLRenderer()
        {
            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);
            GLVersion = major * 100 + minor * 10;

            KHRDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_Debug");

            CompatibilityProfile = (GL.GetInteger((GetPName)All.ContextProfileMask) & (int)All.ContextCompatibilityProfileBit) != 0;
        }
        public static int GenTexture()
        {
            return GL.GenTexture();
        }
        public static int GenVertexArray(string label)
        {
            int va = GL.GenVertexArray();
            GL.BindVertexArray(va);
            LabelObject(ObjectLabelIdentifier.VertexArray, va, label);
            return va;
        }
        public static int GenBuffer(BufferTarget tgt, int size, string label, BufferUsageHint hint)
        {
            int buf = GL.GenBuffer();
            GL.BindBuffer(tgt, buf);
            LabelObject(ObjectLabelIdentifier.Buffer, buf, label);
            GL.BufferData(tgt, size, IntPtr.Zero, hint);
            return buf;
        }
        public static int CreateProgram(string name, string vertexSource, string fragmentSource)
        {
            int program = GL.CreateProgram();
            LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");

            int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
            int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSource);

            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if(success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Logger.CoreDebug($"GL.LinkProgram had info log [{name}]:\n{info}");
            }
            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
            return program;
        }
        public static int CompileShader(string name, ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {name}");
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Logger.CoreDebug($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
            }
            return shader;
        }
        public static void CheckGLError(string title)
        {
            ErrorCode error;
            int i = 1;
            while((error = GL.GetError()) != ErrorCode.NoError)
            {
                Logger.CoreDebug($"{title} ({i++}): {error}");
            }
        }
        public static void LabelObject(ObjectLabelIdentifier objectLabelId, int glObject, string name)
        {
            if(KHRDebugAvailable)
                GL.ObjectLabel(objectLabelId, glObject, name.Length, name);
        }

        public static bool IsExtensionSupported(string name)
        {
            int n = GL.GetInteger(GetPName.NumExtensions);
            for(int i=0; i<n; i++)
            {
                string ext = GL.GetString(StringNameIndexed.Extensions, i);
                if(ext == name) return true;
            }
            return false;
        }

        public static void SaveState()
        {
            // Get intial state.
            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
            int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

            if (GLVersion <= 310 || CompatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }
    }
}
