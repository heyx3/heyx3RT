#pragma once

#include "MaterialValue.h"


namespace RT
{
    EXPORT_RT_LIST(const MaterialValue*);
    EXPORT_RT_LIST(MaterialValue::Ptr);


    //Handles the serialization/deserialization of MaterialValue nodes.
    //If deserializing (i.e. "reading"), call "GetRootVals()"
    //    to get the root nodes of the deserialized graph.
    struct RT_API MaterialValueGraph : public ISerializable
    {
    public:

        MaterialValueGraph() { }
        MaterialValueGraph(const List<const MaterialValue*>& _rootVals)
            : OUT_rootVals(_rootVals) { }


        //Returns the list of root vals after deserialization.
        const List<MaterialValue::Ptr>& GetRootVals() const { return IN_rootVals; }

        virtual void WriteData(DataWriter& data) const override;
        virtual void ReadData(DataReader& data) override;

    private:

        List<const MaterialValue*> OUT_rootVals;
        List<MaterialValue::Ptr> IN_rootVals;
    };
}