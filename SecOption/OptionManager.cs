﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAS5CodeDisasembler.SecOption
{
    class OptionManager
    {
        SecOptionMap? _secOptionMap;

        public OptionManager(byte[]? input)
        {
            if (input == null)
            {
                return;
            }
            using var reader = new BinaryReader(new MemoryStream(input));
            //Must be an OptionMap
            Trace.Assert(reader.ReadByte() == 0);
            _secOptionMap = new SecOptionMap(reader);
        }

        public byte[] Save()
        {
            if(_secOptionMap == null)
            {
                return [];
            }
            var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            _secOptionMap.Save(writer);
            return ms.ToArray();
        }

        public void UpdateExportFuncAddr(Dictionary<long, long> addresses)
        {
            if(_secOptionMap != null 
                && _secOptionMap.Map.TryGetValue("EXPORT_FUNS", out var val) 
                && val is SecOptionMap mapVal)
            {
                foreach(var k in mapVal.Map.Keys)
                {
                    if (mapVal.Map[k] is not SecOptionInteger optionInt)
                    {
                        continue;
                    }
                    if(addresses.TryGetValue(optionInt.Value, out var newAddress))
                    {
                        optionInt.Value = Convert.ToInt32(newAddress);
                    }
                    else
                    {
                        throw new Exception("Unknown jmp addr.");
                    }
                }
            }
        }

    }
}
