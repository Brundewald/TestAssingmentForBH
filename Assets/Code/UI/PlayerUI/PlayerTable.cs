using System.Collections.Generic;
using UnityEngine;

namespace Code.UI.PlayerUI
{
    public sealed class PlayerTable: MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private List<TableField>  _tableFields;

        public List<TableField> TableFields => _tableFields;
    }
}