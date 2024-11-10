```mermaid
%% Read
flowchart TB
    classDef js stroke:#f00

    StartRead[["PokeAByteInstance.Read()"]] -->
    ProcessVariable[["PokeAByteInstance.ProcessVariables()"]] -->
    JsGlobalPreprocessor[js fn: preprocessor]:::js --> StartForEachProperty
    subgraph "For each property"
        StartForEachProperty[["PokeAByteProperty.ProcessLoop() 
        Start"]] -->

        StaticValueQuestion[/StaticValue is defined?/] 
        StaticValueQuestion --> |Yes| StaticValueQuestionY[Set value to StaticValue] --> EndForEachProperty
        StaticValueQuestion --> |No| CalcAddress
        CalcAddress["Calculate the address (if required)"] -->
        ReadBytes["Read bytes from memory (if required)"] -->
        CheckEqual["Have bytes changed?"]
        CheckEqual --> |Yes| ReadValue
        CheckEqual --> |No| EndForEachProperty

        ReadValue[Set the value based off of type] -->
        AfterReadValueExpression[js fn: after-read-value-expression]:::js -->

        RefIsDefinedQ[/Reference is defined?/] 
        RefIsDefinedQ --> |Yes| RefIsDefinedQY[Set value to reference value] --> EndForEachProperty
        RefIsDefinedQ --> |No| EndForEachProperty

        EndForEachProperty[["PokeAByteProperty.ProcessLoop() 
        End"]]
    end
    EndForEachProperty --> JsGlobalPostprocessor
    JsGlobalPostprocessor["js fn: postprocessor"]:::js
```
