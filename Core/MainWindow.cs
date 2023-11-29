using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using ImGuiNET;
using System.ComponentModel;

namespace EZImGui.Core
{
    public class MainWindow : GameWindow
    {
        private float m_Width;
        private float m_Height;

        private List<IPanel> m_Panels;
        private List<IMenu> m_Menus;

        private int m_FramebufferTexture;
        private int m_RectVAO;

        public MainWindow(string title, int  width, int height)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Title = title,
                Size = new OpenTK.Mathematics.Vector2i(width, height),
                StartFocused = true,
                StartVisible = true,
                WindowState = OpenTK.Windowing.Common.WindowState.Normal,
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                Profile = OpenTK.Windowing.Common.ContextProfile.Core,
                APIVersion = new Version(3,3)
            })
       {
            CenterWindow();
            m_Height = Size.Y;
            m_Width = Size.X;

            m_Panels = new List<IPanel>();
            m_Menus = new List<IMenu>();
        }
    
        public void AddPanel(IPanel p) { m_Panels.Add(p); }
        public void RemovePanel(IPanel p) { m_Panels.Remove(p); }
        public void AddMenu(IMenu p) {  m_Menus.Add(p); }
        public void RemoveMenu(IMenu p) { m_Menus.Remove(p); }

        protected override void OnLoad()
        {
            m_FramebufferTexture = OpenGLRenderer.GenTexture();
            GL.ClearColor(new Color4(0.5f, 0.5f, 0.5f, 1.0f));
            GL.LineWidth(2.0f);
            GL.PointSize(5f);
            ImGuiController.WindowResized(ClientSize.X, ClientSize.Y);
            base.OnLoad();
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            m_Height = e.Height;
            m_Width = e.Width;
            GL.DeleteFramebuffer(m_FramebufferTexture);
            GL.Viewport(0,0, ClientSize.X, ClientSize.Y);
            ImGuiController.WindowResized(ClientSize.X, ClientSize.Y);
            base.OnResize(e);
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, m_FramebufferTexture);
            GL.BindVertexArray(m_RectVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 999);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            ImGuiController.Update(this, (float)args.Time);
            ImGui.DockSpaceOverViewport();
            ImGui.BeginMainMenuBar();
            foreach(IMenu menu in m_Menus)
            {
                menu.RenderMenu();
            }
            ImGui.EndMainMenuBar();
            foreach(IPanel panel in m_Panels)
            {
                panel.Render();
            }
            ImGuiController.Render();
            OpenGLRenderer.CheckGLError("End of frame");
            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            ImGuiController.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            ImGuiController.MouseScroll(e.Offset);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            App.Stop();
            base.OnClosing(e);
        }
    }
}
