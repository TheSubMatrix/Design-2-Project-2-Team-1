using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplayHandler : MonoBehaviour
{
    EventBinding<InventoryData> m_inventoryDataEvent;
    void OnEnable()
    {
        m_inventoryDataEvent = new EventBinding<InventoryData>(OnInventoryDataUpdate);
        EventBus<InventoryData>.Register(m_inventoryDataEvent);
    }
    void OnDisable()
    {
        EventBus<InventoryData>.Deregister(m_inventoryDataEvent);
    }
    
    [Serializable]
    struct InventorySlotData
    {
        public Image m_slotDisplay;
        public TMP_Text m_slotCountText;
    }
    
    [SerializeField] List<InventorySlotData> m_inventorySlotData;

    void OnInventoryDataUpdate(InventoryData data)
    {
        for (int i = 0; i < m_inventorySlotData.Count; i++)
        {
            if(i > data.Slots.Length - 1) break;
            if (data.Slots[i].Item?.ItemIcon == null)
            {
                m_inventorySlotData[i].m_slotDisplay.enabled = false;
                m_inventorySlotData[i].m_slotDisplay.sprite = null;
            }
            else
            {
                m_inventorySlotData[i].m_slotDisplay.enabled = true;
                m_inventorySlotData[i].m_slotDisplay.sprite = data.Slots[i].Item.ItemIcon;
            }
            m_inventorySlotData[i].m_slotCountText.text = data.Slots[i].Amount <= 0 ? string.Empty : data.Slots[i].Amount.ToString();
        }
    }
    
}
