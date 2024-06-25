using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public enum ResultType {
    Win,
    Lose,
    Draw
}

public class ResultTextUI : MonoBehaviour
{
    [SerializeField] RectTransform winT;
    [SerializeField] RectTransform loseT;
    [SerializeField] RectTransform drawT;

    public void Show(ResultType type) {
        RectTransform textT = null;

        switch (type)
        {
            case ResultType.Win:
                textT = winT;
                break;
            case ResultType.Lose:
                textT = loseT;
                break;
            case ResultType.Draw:
                textT = drawT;
                break;
        }

        if (textT == null) return;

        
        CanvasGroup group = textT.GetComponent<CanvasGroup>();
        group.alpha = 0;
        textT.localScale = new Vector3(0.5f,0.5f,0.5f);

        Sequence domi = DOTween.Sequence();
        domi.Join(group.DOFade(1, 0.3f).SetEase(Ease.OutQuad));
        domi.Join(textT.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad));

        domi.AppendInterval(1);

        domi.Append(group.DOFade(0, 0.3f).SetEase(Ease.OutQuad));
        domi.Join(textT.DOScale(new Vector3(0.5f,0.5f,0.5f), 0.3f).SetEase(Ease.OutQuad));

        print("show : " + type);
    }
}
