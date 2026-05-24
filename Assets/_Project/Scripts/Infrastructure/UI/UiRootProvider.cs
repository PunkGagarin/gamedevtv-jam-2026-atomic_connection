using UnityEngine;

namespace _Project.Scripts.Infrastructure.UI
{
    public class UiRootProvider : IUiRootProvider
    {
        public RectTransform UIRoot { get; private set; }

        public void SetUIRoot(RectTransform uiRoot)
        {
            UIRoot = uiRoot;
        }
    }
}
