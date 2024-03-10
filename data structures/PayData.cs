using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Nimrod;
using UnityEngine;

[System.Serializable]
public class PayData {
    public enum DataType { pay, personnel, password, location, objective, unknown }
    public static DataType[] acceptableRandomTypes = new DataType[]{
        DataType.pay, DataType.personnel, DataType.password, DataType.location
    };
    public DataType type;
    public string filename;
    public int value;

    static Grammar grammar;

    public static void MaybeInitializeGrammar() {
        if (grammar == null) {
            grammar = new Grammar();
            grammar.Load("paydata");
        }
    }
    public static PayData RandomPaydata() {
        MaybeInitializeGrammar();
        return new PayData {
            type = RandomDataType(),
            filename = grammar.Parse("{filename}"),
            value = UnityEngine.Random.Range(50, 3000)
        };
    }
    public static PayData DummyPaydata() {
        return new PayData {
            type = DataType.unknown,
            filename = "???",
            value = 0
        };
    }
    public static PayData DummyObjective(ObjectiveData objective) {
        return new PayData {
            type = DataType.objective,
            filename = objective.targetPaydata.filename,
            value = 0
        };
    }

    public static DataType RandomDataType() {
        return Toolbox.RandomFromList(acceptableRandomTypes);
    }

}