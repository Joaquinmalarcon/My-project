using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownController : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    private int ultimoIndiceValido = 1;
    private readonly HashSet<int> titulosIndices = new HashSet<int> { 0, 2 };

    void Start()
    {
        dropdown.options.Clear();

        dropdown.options.Add(new TMP_Dropdown.OptionData("── Aulas ──"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("IBC 2-1"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("── Entrada/Salida ──"));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Entrada IBC"));

        dropdown.value = 1;
        dropdown.RefreshShownValue();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    public void OnDropdownChanged(int index)
    {
        if (titulosIndices.Contains(index))
        {
            dropdown.SetValueWithoutNotify(ultimoIndiceValido);
            return;
        }

        ultimoIndiceValido = index;

        switch (index)
        {
            case 1:
                Debug.Log("Seleccionaste: IBC 2-1");
                break;
            case 3:
                Debug.Log("Seleccionaste: Entrada IBC");
                break;
        }
    }
}