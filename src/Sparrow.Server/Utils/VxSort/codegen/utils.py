from datetime import datetime

native_size_map = {
    "int": 4,
    "uint": 4,
    "float": 4,
    "long": 8,
    "ulong": 8,
    "double": 8,
}

capital_type_map = {
    "int": "Int32",
    "uint": "UInt32",
    "float": "Float",
    "long": "Int64",
    "ulong": "UInt64",
    "double": "Double",
}

native_types = ["int", "uint", "float", "long", "ulong", "double"]


def next_power_of_2(v):
    v = v - 1
    v |= v >> 1
    v |= v >> 2
    v |= v >> 4
    v |= v >> 8
    v |= v >> 16
    v = v + 1
    return int(v)


def autogenerated_blabber():
        return f"""/////////////////////////////////////////////////////////////////////////////
//
// This file was auto-generated by a tool at {datetime.now().strftime("%F %H:%M:%S")}
//
// It is recommended you DO NOT directly edit this file but instead edit
// the code-generator that generated this source file instead.
/////////////////////////////////////////////////////////////////////////////
"""
