﻿using Microsoft.Xna.Framework;

namespace Pulsar.Graphics
{
    /// <summary>
    /// Interface defining a renderable object in the scene graph
    /// </summary>
    public interface IRenderable
    {
        #region Properties

        /// <summary>
        /// Get the ID of this renderable
        /// </summary>
        uint Id { get; }

        /// <summary>
        /// Get the name of this instance
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get or set a boolean indicating if this instance use instancing
        /// </summary>
        bool UseInstancing { get; set; }

        /// <summary>
        /// Get the ID of the render queue to be attached on
        /// </summary>
        int RenderQueueId { get; }

        /// <summary>
        /// Get the full transform matrix
        /// </summary>
        Matrix Transform { get; }

        /// <summary>
        /// Get the rendering info
        /// </summary>
        RenderingInfo RenderInfo { get; }

        /// <summary>
        /// Get the material associated to this renderable object
        /// </summary>
        Material Material { get; }

        #endregion
    }
}
