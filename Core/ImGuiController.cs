using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
namespace EZImGui.Core
{
    public static class ImGuiController
    {
        private static readonly ImGuiIOPtr IO;

        private static bool m_FrameBegun;
        private static int m_VertexArray;
        private static int m_VertexBuffer;
        private static int m_VertexBufferSize;
        private static int m_IndexBuffer;
        private static int m_IndexBufferSize;
        private static int m_FontTexture;
        private static int m_Shader;
        private static int m_ShaderFontTextureLocation;
        private static int m_ShaderProjectionMatrixLocation;
        private static int m_WindowHeight;
        private static int m_WindowWidth;
        private static System.Numerics.Vector2 m_ScaleFactor = System.Numerics.Vector2.One;

        static ImGuiController()
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            IO = ImGui.GetIO();
            IO.Fonts.AddFontDefault();
            
            AddBackendFlag(ImGuiBackendFlags.RendererHasVtxOffset);
            AddConfigFlag(ImGuiConfigFlags.DockingEnable);
            AddConfigFlag(ImGuiConfigFlags.ViewportsEnable);

            Initialize();
            SetPerFrameImGuiData(1f / 60f);
            ImGui.NewFrame();
            m_FrameBegun = true;
        }
        #region Public
        public static void Render()
        {
            if(m_FrameBegun)
            {
                m_FrameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }
        public static void Update(GameWindow wnd, float deltaSeconds)
        {
            if (m_FrameBegun)
                ImGui.Render();
            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);
            m_FrameBegun = true;
            ImGui.NewFrame();
        }
        public static void WindowResized(int width, int height)
        {
            m_WindowHeight = height;
            m_WindowWidth = width;
        }
        #region Flags
        public static void AddBackendFlag(ImGuiBackendFlags flag)
        {
            IO.BackendFlags |= flag;
        }
        public static void RemoveBackendFlag(ImGuiBackendFlags flag)
        {
            IO.BackendFlags &= ~flag;
        }
        public static void AddConfigFlag(ImGuiConfigFlags flags)
        {
            IO.ConfigFlags |= flags;
        }
        public static void RemoveConfigFlag(ImGuiConfigFlags flag)
        {
            IO.ConfigFlags &= ~flag;
        }
        #endregion Flags
        #endregion Public

        #region Private
        private static void Initialize()
        {
            m_VertexBufferSize = 10000;
            m_IndexBufferSize = 2000;

            int prevVao = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

            m_VertexArray = OpenGLRenderer.GenVertexArray("ImGui");
            m_VertexBuffer = OpenGLRenderer.GenBuffer(BufferTarget.ArrayBuffer, m_VertexBufferSize, "VBO: ImGui", BufferUsageHint.DynamicDraw);
            m_IndexBuffer = OpenGLRenderer.GenBuffer(BufferTarget.ElementArrayBuffer, m_IndexBufferSize, "EBO: ImGui", BufferUsageHint.DynamicDraw);

            OpenGLRenderer.CreateProgram("ImGui", VertexSource, FragmentSource);

            m_ShaderProjectionMatrixLocation = GL.GetUniformLocation(m_Shader, "projection_matrix");
            m_ShaderFontTextureLocation = GL.GetUniformLocation(m_Shader, "in_fontTexture");

            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(prevVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

            OpenGLRenderer.CheckGLError("End of ImGui setup");

        }
        private static void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            GLState.SaveState();
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }
            if (OpenGLRenderer.GLVersion <= 310 || OpenGLRenderer.CompatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            // Bind the element buffer (thru the VAO) so that we can resize it.
            GL.BindVertexArray(m_VertexArray);
            // Bind the vertex buffer so that we can resize it.
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_VertexBuffer);
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > m_VertexBufferSize)
                {
                    int newSize = (int)Math.Max(m_VertexBufferSize * 1.5f, vertexSize);

                    GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    m_VertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {m_VertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > m_IndexBufferSize)
                {
                    int newSize = (int)Math.Max(m_IndexBufferSize * 1.5f, indexSize);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    m_IndexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui index buffer to new size {m_IndexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
            0.0f,
                IO.DisplaySize.X,
                IO.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            GL.UseProgram(m_Shader);
            GL.UniformMatrix4(m_ShaderProjectionMatrixLocation, false, ref mvp);
            GL.Uniform1(m_ShaderFontTextureLocation, 0);
            OpenGLRenderer.CheckGLError("Projection");

            GL.BindVertexArray(m_VertexArray);
            OpenGLRenderer.CheckGLError("VAO");

            draw_data.ScaleClipRects(IO.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[n];

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                OpenGLRenderer.CheckGLError($"Data Vert {n}");

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                OpenGLRenderer.CheckGLError($"Data Idx {n}");

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        OpenGLRenderer.CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, m_WindowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        OpenGLRenderer.CheckGLError("Scissor");

                        if ((IO.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        OpenGLRenderer.CheckGLError("Draw");
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
            GLState.RestoreState();
        }
        
        private static void SetPerFrameImGuiData(float deltaSeconds)
        {
            IO.DisplaySize = new System.Numerics.Vector2(
                m_WindowHeight / m_ScaleFactor.X,
                m_WindowWidth / m_ScaleFactor.Y);
            IO.DisplayFramebufferScale = m_ScaleFactor;
            IO.DeltaTime = deltaSeconds;
        }

        static readonly List<char> PressedChars = new();

        [RequiresDynamicCode("Calls System.Enum.GetValues(Type)")]
        private static void UpdateImGuiInput(GameWindow wnd)
        {
            MouseState MouseState = wnd.MouseState;
            KeyboardState KeyboardState = wnd.KeyboardState;

            IO.MouseDown[0] = MouseState[MouseButton.Left];
            IO.MouseDown[1] = MouseState[MouseButton.Right];
            IO.MouseDown[2] = MouseState[MouseButton.Middle];
            IO.MouseDown[3] = MouseState[MouseButton.Button4];
            IO.MouseDown[4] = MouseState[MouseButton.Button5];

            var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
            var point = screenPoint;//wnd.PointToClient(screenPoint);
            IO.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }
                IO.AddKeyEvent(KeyTranslator.Translate(key), KeyboardState.IsKeyDown(key));
            }

            foreach (var c in PressedChars)
            {
                IO.AddInputCharacter(c);
            }
            PressedChars.Clear();

            IO.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
            IO.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
            IO.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            IO.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
        }
        static internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }
        static internal void MouseScroll(Vector2 offset)
        {
            IO.MouseWheel = offset.Y;
            IO.MouseWheelH = offset.X;
        }

        #endregion Private

        #region Shaders
        const string VertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
        const string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

        #endregion
        #region NoTouchy
        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public static void RecreateFontDeviceTexture()
        {
            IO.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

            m_FontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_FontTexture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
            OpenGLRenderer.LabelObject(ObjectLabelIdentifier.Texture, m_FontTexture, "ImGui Text Atlas");

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            // Restore state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);

            IO.Fonts.SetTexID((IntPtr)m_FontTexture);

            IO.Fonts.ClearTexData();
        }
        #endregion
    }
}
