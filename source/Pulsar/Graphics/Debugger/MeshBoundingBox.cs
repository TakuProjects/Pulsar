﻿using System;

using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Pulsar.Game;
using Pulsar.Core;
using Pulsar.Assets.Graphics.Models;
using Pulsar.Assets.Graphics.Materials;
using Pulsar.Graphics.SceneGraph;
using Pulsar.Graphics.Rendering;

namespace Pulsar.Graphics.Debugger
{
    /// <summary>
    /// Class to create an draw a bounding box
    /// Used only for debugging
    /// </summary>
    internal sealed class MeshBoundingBox : IRenderable
    {
        #region Fields

        private const string materialName = "MeshBoundingBox_Material";
        private const int verticesCount = 24;
        private const int primitiveCount = 12;

        private VertexBufferObject vbo;
        private IndexBufferObject ibo;
        private RenderingInfo renderInfo;
        private Material material;
        private short[] indices = new short[MeshBoundingBox.verticesCount];
        private VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[MeshBoundingBox.verticesCount];

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor of MeshBoundingBox class
        /// </summary>
        internal MeshBoundingBox()
        {
            this.InitBuffers();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize all the buffers
        /// </summary>
        private void InitBuffers()
        {
            GraphicsEngineService engineService = GameApplication.GameServices.GetService(typeof(IGraphicsEngineService)) as GraphicsEngineService;
            if (engineService == null)
            {
                throw new ArgumentException("GraphicsEngineService cannot be found");
            }

            BufferManager bufferMngr = engineService.Engine.BufferManager;
            VertexData vData = new VertexData();
            this.vbo = bufferMngr.CreateVertexBuffer(BufferType.DynamicWriteOnly, typeof(VertexPositionNormalTexture),
                MeshBoundingBox.verticesCount);
            vData.SetBinding(this.vbo);

            IndexData iData = new IndexData();
            this.ibo = bufferMngr.CreateIndexBuffer(BufferType.DynamicWriteOnly, IndexElementSize.SixteenBits,
                MeshBoundingBox.verticesCount);
            this.ibo.SetData(this.indices);
            iData.indexBuffer = ibo;

            for (short i = 0; i < MeshBoundingBox.verticesCount; i++)
            {
                indices[i] = i;
            }

            this.renderInfo = new RenderingInfo()
            {
                primitive = PrimitiveType.LineList,
                vertexData = vData,
                indexData = iData,
                vertexCount = MeshBoundingBox.verticesCount,
                triangleCount = MeshBoundingBox.primitiveCount
            };
            this.material = MaterialManager.Instance.LoadDefault();
            this.material.DiffuseColor = Color.White;
        }

        /// <summary>
        /// Update the buffers
        /// </summary>
        /// <param name="box">Boundig box used to update buffers data</param>
        internal void UpdateBox(ref BoundingBox box)
        {
            Vector3 xOffset = new Vector3((box.Max.X - box.Min.X), 0, 0);
            Vector3 yOffset = new Vector3(0, (box.Max.Y - box.Min.Y), 0);
            Vector3 zOffset = new Vector3(0, 0, (box.Max.Z - box.Min.Z));
            Vector3 minOpposite = box.Min + (xOffset + zOffset);
            Vector3 maxOpposite = box.Max - (xOffset + zOffset);

            //// Top
            this.vertices[0].Position = box.Min;
            this.vertices[1].Position = box.Min + xOffset;
            this.vertices[2].Position = box.Min;
            this.vertices[3].Position = box.Min + zOffset;
            this.vertices[4].Position = minOpposite;
            this.vertices[5].Position = box.Min + xOffset;
            this.vertices[6].Position = minOpposite;
            this.vertices[7].Position = box.Min + zOffset;

            //// Bottom
            this.vertices[8].Position = box.Max;
            this.vertices[9].Position = box.Max - xOffset;
            this.vertices[10].Position = box.Max;
            this.vertices[11].Position = box.Max - zOffset;
            this.vertices[12].Position = maxOpposite;
            this.vertices[13].Position = box.Max - xOffset;
            this.vertices[14].Position = maxOpposite;
            this.vertices[15].Position = box.Max - zOffset;

            //// Sides
            this.vertices[16].Position = box.Min;
            this.vertices[17].Position = box.Min + yOffset;
            this.vertices[18].Position = box.Min + xOffset;
            this.vertices[19].Position = (box.Min + xOffset) + yOffset;
            this.vertices[20].Position = box.Max;
            this.vertices[21].Position = box.Max - yOffset;
            this.vertices[22].Position = box.Max - xOffset;
            this.vertices[23].Position = (box.Max - xOffset) - yOffset;

            this.vbo.SetData(this.vertices);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the name of this instace
        /// </summary>
        public string Name 
        {
            get { return "Debug AABB"; } 
        }

        /// <summary>
        /// Get or set a boolean to enable instancing
        /// </summary>
        public bool UseInstancing 
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Get the batch ID of this instance
        /// </summary>
        public uint BatchID 
        {
            get { return this.renderInfo.id; } 
        }

        /// <summary>
        /// Get the render qeue ID of this instance
        /// </summary>
        public int RenderQueueID 
        {
            get { return (int)RenderQueueGroupID.Default; } 
        }

        /// <summary>
        /// Get the transform matrix of this instance
        /// </summary>
        public Matrix Transform 
        {
            get { return Matrix.Identity; }
        }

        /// <summary>
        /// Get the rendering info of this instance
        /// </summary>
        public RenderingInfo RenderInfo 
        {
            get { return this.renderInfo; }
        }

        /// <summary>
        /// Get the material associated to this instance
        /// </summary>
        public Material Material
        {
            get { return this.material; }
        }

        #endregion
    }
}
