using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class EmptyData : IMessageData
{
    public void FromBytes(BinaryReader reader)
    {
        
    }

    public void Write(BinaryWriter writer)
    {

    }
}