using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI chargesText;
    public TextMeshProUGUI attackTypeText;

    [SerializeField]
    private Player _player;

    [SerializeField]
    private PlayerAttackManager _attackManager;

    // Update is called once per frame
    void Update()
    {
        string elementalCharges = new string('-', _attackManager.GetCurrentChargeCount()) + new string(' ', 10 - _attackManager.GetCurrentChargeCount());
        chargesText.text = elementalCharges;


        attackTypeText.text = _attackManager.GetCurrentAttack().ToUpper();
        if (attackTypeText.text.Equals("TEMPEST"))
        {
            attackTypeText.color = new Color(147f / 255f, 0f, 254f / 255f);

        }
    }
}
