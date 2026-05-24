using UnityEngine;

namespace _Project.Scripts.Infrastructure.UI
{
    public interface IUiRootProvider
    {
        RectTransform UIRoot { get; }
        void SetUIRoot(RectTransform uiRoot);
    }
}
