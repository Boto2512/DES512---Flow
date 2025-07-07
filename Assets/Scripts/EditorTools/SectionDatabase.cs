using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SectionDatabase", menuName = "Scriptable Objects/SectionDatabase")]
public class SectionDatabase : ScriptableObject
{
    public List<SectionData> sections = new List<SectionData>();
}
