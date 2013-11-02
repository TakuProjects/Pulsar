﻿using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Pulsar.Graphics.Rendering.RenderingTechnique;
using Pulsar.Graphics.SceneGraph;

namespace Pulsar.Graphics.Rendering
{
    /// <summary>
    /// Performs all rendering operations on a graphics device
    /// </summary>
    public sealed class Renderer : IDisposable
    {
        #region Fields

        private bool _disposed;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private GraphicsDevice _graphicDevice;
        private SpriteBatch _spriteBatch;
        private readonly InstanceBatchManager _instancingManager;
        private readonly IRenderingTechnique _renderingTechnique;
        private readonly FrameDetail _frameDetail = new FrameDetail();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor of the GraphicsRenderer class
        /// </summary>
        /// <param name="deviceManager">Graphics device manager</param>
        internal Renderer(GraphicsDeviceManager deviceManager)
        {
            _graphicsDeviceManager = deviceManager;
            _graphicDevice = deviceManager.GraphicsDevice;
            deviceManager.DeviceCreated += GraphicsDeviceCreated;

            _spriteBatch = new SpriteBatch(_graphicDevice);
            _instancingManager = new InstanceBatchManager(this);
            _renderingTechnique = new SimpleRenderingTechnique(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes ressources
        /// </summary>
        public void Dispose()
        {
            if(_disposed) return;

            _graphicsDeviceManager.DeviceCreated -= GraphicsDeviceCreated;
            if(_spriteBatch != null) _spriteBatch.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// Called each time graphics device is created
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private void GraphicsDeviceCreated(object sender, EventArgs e)
        {
            _graphicDevice = _graphicsDeviceManager.GraphicsDevice;

            if(_spriteBatch != null) _spriteBatch.Dispose();
            _spriteBatch = new SpriteBatch(_graphicDevice);
        }

        /// <summary>
        /// Prepares rendering operations
        /// </summary>
        private void BeginRender()
        {
            _instancingManager.Reset();
            _frameDetail.Reset();
            _graphicDevice.DepthStencilState = DepthStencilState.Default;
        }

        /// <summary>
        /// Ends all rendering operations
        /// </summary>
        /// <param name="vp">Viewport in which the rendering result is sent</param>
        private void EndRender(Viewport vp)
        {
            vp.FrameDetail.Merge(_frameDetail);
        }

        /// <summary>
        /// Resets current graphic device vertex and index buffers
        /// </summary>
        private void UnsetBuffers()
        {
            _graphicDevice.SetVertexBuffer(null);
            _graphicDevice.Indices = null;
        }

        /// <summary>
        /// Clears the back buffer
        /// </summary>
        /// <param name="clearColor">Color used to clear</param>
        internal void Clear(Color clearColor)
        {
            _graphicDevice.Clear(clearColor);
        }

        /// <summary>
        /// Sets a render target on the graphic device
        /// </summary>
        /// <param name="renderTarget">Render target to set</param>
        internal void SetRenderTarget(RenderTarget2D renderTarget)
        {
            _graphicDevice.SetRenderTarget(renderTarget);
        }

        /// <summary>
        /// Removes all render target from the graphic device
        /// </summary>
        internal void UnsetRenderTarget()
        {
            _graphicDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Renders a scene graph into a viewport
        /// </summary>
        /// <param name="vp">Viewport in which the rendering is sent</param>
        /// <param name="cam">Camera representing the point of view</param>
        /// <param name="queue">Queue of objects to render</param>
        internal void Render(Viewport vp, Camera cam, RenderQueue queue)
        {
            BeginRender();

            _renderingTechnique.Render(vp, cam, queue);

            EndRender(vp);
        }

        /// <summary>
        /// Draws all viewports of a RenderTarget into its own render target
        /// </summary>
        /// <param name="renderTarget">RenderTarget to render</param>
        internal void RenderToTarget(RenderTarget renderTarget)
        {
            _graphicDevice.SetRenderTarget(renderTarget.Target);
            if(renderTarget.AlwaysClear) _graphicDevice.Clear(renderTarget.ClearColor);

            ViewportCollection viewports = renderTarget.Viewports;
            if (viewports.Count > 0)
            {
                _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise);
                for (int i = 0; i < viewports.Count; i++)
                {
                    Viewport vp = viewports[i];
                    Rectangle rect = new Rectangle(vp.RealLeft, vp.RealTop, vp.RealWidth, vp.RealHeight);
                    _spriteBatch.Draw(vp.RenderTarget, rect, Color.White);
                }
                _spriteBatch.End();
            }

            _graphicDevice.SetRenderTarget(null);
        }

        /// <summary>
        /// Draws a texture on the entire screen
        /// </summary>
        /// <param name="texture">Texture to render</param>
        internal void DrawFullQuad(Texture2D texture)
        {
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullCounterClockwise);
            PresentationParameters presentation = _graphicDevice.PresentationParameters;
            Rectangle rect = new Rectangle(0, 0, presentation.BackBufferWidth, presentation.BackBufferHeight);
            _spriteBatch.Draw(texture, rect, Color.White);
            _spriteBatch.End();

            _frameDetail.AddDrawCall(4, 2, 1);
        }

        /// <summary>
        /// Renders one 3D geometric shape
        /// </summary>
        /// <param name="geometry"></param>
        internal void Draw(IRenderable geometry)
        {
            RenderingInfo renderingInfo = geometry.RenderInfo;

            _graphicDevice.SetVertexBuffers(renderingInfo.VertexData.VertexBindings);
            if (renderingInfo.UseIndexes)
            {
                IndexData indexData = renderingInfo.IndexData;
                _graphicDevice.Indices = indexData.HardwareBuffer;
                _graphicDevice.DrawIndexedPrimitives(renderingInfo.PrimitiveType, 0, 0,
                    renderingInfo.VertexCount, indexData.StartIndex, renderingInfo.PrimitiveCount);
            }
            else
                _graphicDevice.DrawPrimitives(renderingInfo.PrimitiveType, 0, renderingInfo.PrimitiveCount);
            UnsetBuffers();

            _frameDetail.AddDrawCall((uint)renderingInfo.VertexCount, (uint)renderingInfo.PrimitiveCount, 1);
        }

        /// <summary>
        /// Draws a geometry batch
        /// </summary>
        /// <param name="batch">Bztch of geometric shapes</param>
        internal void DrawInstancedGeometry(InstanceBatch batch)
        {
            if (batch.InstanceCount == 0)
                return;

            RenderingInfo renderInfo = batch.RenderInfo;
            _graphicDevice.SetVertexBuffers(renderInfo.VertexData.VertexBindings);
            IndexData indexData = renderInfo.IndexData;
            _graphicDevice.Indices = indexData.HardwareBuffer;
            _graphicDevice.DrawInstancedPrimitives(renderInfo.PrimitiveType, 0, 0, renderInfo.VertexCount,
                indexData.StartIndex, renderInfo.PrimitiveCount, batch.InstanceCount);
            UnsetBuffers();

            int instanceCount = batch.InstanceCount;
            _frameDetail.AddDrawCall((uint)(renderInfo.VertexCount * instanceCount), (uint)(renderInfo.PrimitiveCount * instanceCount), 
                (uint)instanceCount);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the graphic device used by this renderer
        /// </summary>
        internal GraphicsDevice GraphicsDevice
        {
            get { return _graphicDevice; }
        }

        /// <summary>
        /// Gets the InstanceBatch manager
        /// </summary>
        internal InstanceBatchManager InstancingManager
        {
            get { return _instancingManager; }
        }

        #endregion
    }
}
