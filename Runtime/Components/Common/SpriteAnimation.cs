using UnityEngine;
using UnityEngine.UI;

namespace Larje.UI
{
    [RequireComponent(typeof(Image))]
    public sealed class SpriteAnimation : MonoBehaviour
    {
        [SerializeField] private float frameDuration = 0.1f;
        [SerializeField] private Sprite[] sprites = new Sprite[0];

        private int _frame;
        private float _elapsed;
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Update()
        {
            if (sprites.Length == 0 || frameDuration <= 0f)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            while (_elapsed >= frameDuration)
            {
                _elapsed -= frameDuration;
                _frame = (_frame + 1) % sprites.Length;
                _image.sprite = sprites[_frame];
            }
        }
    }
}
