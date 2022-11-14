using System.Collections;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Cards
{
    public class Hero : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer _avatar;
        [SerializeField]
        private TextMeshPro _name;
        [SerializeField]
        private TextMeshPro _hp;
        [SerializeField]
        private TextMeshPro _manaValue;
        [SerializeField]
        private TextAsset _heroesJsonFile;
        [SerializeField]
        private HeroClass _class;
        [SerializeField]
        private Material[] _avatarMaterials;

        private int _currentHp = 0;
        private int _mana = 0;

        private void Awake()
        {
            SetupHero();
        }

        private void SetupHero()
        {
            var classesData = JsonUtility.FromJson<HeroClasses>(_heroesJsonFile.text);
            HeroData heroData;
            switch (_class)
            {
                case HeroClass.Hunter:
                    heroData = classesData.Hunter;
                    break;
                case HeroClass.Mage:
                    heroData = classesData.Mage;
                    break;
                case HeroClass.Priest:
                    heroData = classesData.Priest;
                    break;
                case HeroClass.Warrior:
                default:
                    heroData = classesData.Warrior;
                    break;
            }

            _avatar.material = _avatarMaterials.FirstOrDefault(material => material.name == heroData.AvatarMaterial);
            _name.text = heroData.Name;
            _hp.text = heroData.Hp.ToString();
            _currentHp = heroData.Hp;
            _manaValue.text = _mana + "/10";
        }

        public void AddDamage(int damage)
        {
            _currentHp -= damage;
            _hp.text = _currentHp.ToString();
            Debug.Log("Damage to " + _name.text + " : " + damage);
            StartCoroutine(AnimateDamage());
        }

        private IEnumerator AnimateDamage()
        {
            var durationSec = 0.2f;
            var upScale = new Vector3(1.1f, 1.1f, 1.1f);
            var initialScale = transform.localScale;

            for (float time = 0; time < durationSec * 2; time += Time.deltaTime)
            {
                float progress = Mathf.PingPong(time, durationSec) / durationSec;
                transform.localScale = Vector3.Lerp(initialScale, upScale, progress);
                yield return null;
            }
            transform.localScale = initialScale;
            if (_currentHp <= 0)
            {
                Destroy(gameObject);
                Debug.Log(_name.text + " killed");
                EditorApplication.isPaused = true;
            }
        }

        public void IncrementMana()
        {
            _mana = Mathf.Min(10, _mana + 1);
            _manaValue.text = _mana + "/10";
        }

        public void DecrementMana()
        {
            _mana = Mathf.Max(0, _mana - 1);
            _manaValue.text = _mana + "/10";
        }
    }
}
