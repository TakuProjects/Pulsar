﻿using System;

namespace Pulsar.Graphics
{
    public sealed class RenderQueueKey
    {
        #region Fields

        private const ulong PassFirstMask = 0x000000FFFF000000;
        private const ulong PassLastMask = 0x000000000000FFFF;
        private const ulong MaterialFirstMask = 0x00FFFF0000000000;
        private const ulong MaterialLastMask = 0x00000000FFFF0000;
        private const ulong DepthFirstMask = 0x00FFFFFF00000000;
        private const ulong DepthLastMask = 0x0000000000FFFFFF;
        private const ulong TransparencyMask = 0x100000000000000;
        private const ulong GroupMask = 0x1E00000000000000;

        private ulong _material;
        private ulong _pass;
        private ulong _depth;
        private ulong _transparency;
        private ulong _group;
        private Action _update;
        private ulong _id;

        #endregion

        #region Constructors

        public RenderQueueKey() : this(0u)
        {
        }

        public RenderQueueKey(ulong id)
        {
            _update = UpdateByMaterialPriority;

            _id = id;
            Transparency = Convert.ToBoolean((_id & TransparencyMask) >> 56);
            if(_transparency != 0)
                ExtractByDepthPriority();
            else
                ExtractByMaterialPriority();
        }

        #endregion

        #region Operators

        public static implicit operator ulong(RenderQueueKey key)
        {
            return key.Id;
        }

        public static implicit operator RenderQueueKey(ulong id)
        {
            return new RenderQueueKey(id);
        }

        #endregion

        #region Methods

        private void ExtractByDepthPriority()
        {
            _pass = (_id & PassLastMask);
            _material = (_id & MaterialLastMask) >> 16;
            _depth = (_id & DepthFirstMask) >> 32;
            _group = (_id & GroupMask) >> 57;
        }

        private void ExtractByMaterialPriority()
        {
            _depth = (_id & DepthLastMask);
            _pass = (_id & PassFirstMask) >> 24;
            _material = (_id & MaterialFirstMask) >> 40;
            _group = (_id & GroupMask) >> 57;
        }

        private void UpdateByDepthPriority()
        {
            _id |= _pass;
            _id |= _material << 16;
            _id |= _depth << 32;
            _id |= _transparency << 56;
            _id |= _group << 57;
        }

        private void UpdateByMaterialPriority()
        {
            _id |= _depth;
            _id |= _pass << 24;
            _id |= _material << 40;
            _id |= _transparency << 56;
            _id |= _group << 57;
        }

        #endregion

        #region Properties

        public ulong Id
        {
            get { return _id; }
        }

        public byte Group
        {
            get { return (byte)_group; }
            set
            {
                _group = (ulong)(value & 0x0F);
                _update();
            }
        }
        
        public bool Transparency
        {
            get { return Convert.ToBoolean(_transparency); }
            set
            {
                _transparency = Convert.ToUInt64(value);
                _update = value ? (Action)UpdateByDepthPriority : UpdateByMaterialPriority;
                _update();
            }
        }

        public float Depth
        {
            get { return _depth; }
            set
            {
                Depth24 depth = new Depth24(value);
                _depth = depth.ToUInt64();
                _update();
            }
        }

        public ushort Material
        {
            get { return (ushort) _material; }
            set
            {
                _material = value;
                _update();
            }
        }

        public ushort Pass
        {
            get { return (ushort) _pass; }
            set
            {
                _pass = value;
                _update();
            }
        }

        #endregion
    }
}
