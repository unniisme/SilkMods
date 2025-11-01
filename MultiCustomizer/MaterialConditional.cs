using System;
using System.Collections.Generic;
using System.Text;

namespace MultiCustomizer
{
    /// <summary>
    /// Publisher class that tells multicustomizer whether to load this skin or not
    /// 
    /// Set IsActive to true to set this skin
    /// </summary>
    public class MaterialConditional
    {
        public TextureState state;

        private bool isActive = false;

        /// <summary>
        /// Bool signifying whether this skin is currently active or not
        /// </summary>
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value != isActive)
                {
                    isActive = value;
                    ActivityChangedEvent?.Invoke(state, isActive);
                }
            }
        }

        public Action<TextureState, bool> ActivityChangedEvent;


        public MaterialConditional(TextureState state)
        {
            this.state = state;
        }
    }
}
