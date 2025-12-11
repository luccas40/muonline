using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.Data.BMD;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct MasterSkillTreeData
{
    public ushort ID;
    public ushort Class;
    public byte TreeType;
    public byte ReqPoint;
    public byte MaxPoint;
    public byte Unk1;
    public uint Parent_Skill_1;
    public uint Parent_Skill_2;
    public uint SkillNum;
    public uint Formula;    
}
