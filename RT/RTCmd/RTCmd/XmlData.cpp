#include "XmlData.h"

#include <iostream>

#include <RT.hpp>

#include "tinyxml2.h"


namespace
{
    std::string Combine(const char* a, const char* b)
    {
        std::string str = a;
        str += b;
        return str;
    }

    bool AreEqual(const char* a, const char* b)
    {
        return strcmp(a, b) == 0;
    }

    std::string ToString(int i)
    {
        char str[32];
        return _itoa(i, str, 10);
    }

    void ToString(char* outStr, const Vector3f& v)
    {
        sprintf(outStr, "%f %f %f", v.x, v.y, v.z);
    }
    bool FromString(const char* str, Vector3f& outV)
    {
        float vals[3];
        int currentVal = 0;

        int startI = 0;
        int endI;
        while (currentVal < 3)
        {
            //Error if there weren't at least 3 values.
            if (str[startI] == '\0')
                return false;

            //Get the end of the current number in the string.
            endI = startI;
            while (str[endI] != '\0' && str[endI] != ' ')
                endI += 1;
            endI -= 1;

            //Error if there was no number in this part of the string.
            if (startI > endI)
                return false;

            //Convert to a string.
            vals[currentVal] = (float)atof(&str[startI]);

            currentVal += 1;
            startI = endI + 2;
        }

        outV = Vector3f(vals[0], vals[1], vals[2]);
        return true;
    }
}
#define IS_TYPE(x, type) (typeid(x) == typeid(type))


std::string XmlData::FromFile(const std::string& filePath,
                              std::vector<Shape*>& outShapes,
                              std::vector<Material*>& outMats,
                              SkyMaterial** outSkyMat)
{
    *outSkyMat = nullptr;

    char str[64];
    tinyxml2::XMLDocument doc;
    
    tinyxml2::XMLError err = doc.LoadFile(filePath.c_str());
    if (err != tinyxml2::XML_NO_ERROR)
    {
        sprintf(str, "%d", (int)err);
        return Combine("Error loading document: tinyxml2 error code ", str);
    }


    tinyxml2::XMLElement* root = doc.FirstChildElement();

    int nShapes;
    err = root->QueryIntAttribute("NShapes", &nShapes);
    if (err != tinyxml2::XML_NO_ERROR)
    {
        return "Couldn't get 'NShapes' integer attribute in Root element";
    }

    outShapes.resize(nShapes);
    outMats.resize(nShapes);
    for (int i = 0; i < nShapes; ++i)
    {
        std::string iStr = ToString(i);

        //Get the shape.

        tinyxml2::XMLElement* shapeEl = root->FirstChildElement(Combine("Shape", iStr.c_str()).c_str());
        if (shapeEl == nullptr)
        {
            return Combine("Couldn't find shape with index ", iStr.c_str());
        }

        Transform tr;
        tinyxml2::XMLElement* shapeTr = shapeEl->FirstChildElement("Transform");
        if (shapeTr == nullptr)
            return Combine("Couldn't find Transform element for shape index ", iStr.c_str());

        const char* cStr = shapeTr->Attribute("Pos");
        Vector3f v;
        if (cStr == nullptr || !FromString(cStr, v))
            return Combine("Couldn't find Pos attribute for shape's transform, index ", iStr.c_str());
        tr.SetPos(v);

        cStr = shapeTr->Attribute("Scale");
        if (cStr == nullptr || !FromString(cStr, v))
            return Combine("Couldn't find Scale attribute for shape's transform, index ", iStr.c_str());
        tr.SetScale(v);
        
        cStr = shapeTr->Attribute("RotAxis");
        if (cStr == nullptr || !FromString(cStr, v))
            return Combine("Couldn't find RotAxis attribute for shape's transform, index ", iStr.c_str());
        float angle;
        if (shapeTr->QueryFloatAttribute("RotAngle", &angle) != tinyxml2::XML_NO_ERROR)
            return Combine("Couldn't get RotAngle attribute for shape's transform, index ", iStr.c_str());
        tr.SetRot(Quaternion(v, angle));

        const char* shapeType = shapeEl->Attribute("Type");
        if (AreEqual(shapeType, "Sphere"))
        {
            Sphere* sph = new Sphere();
            sph->Tr = tr;
            outShapes[i] = sph;
        }
        else if (AreEqual(shapeType, "Plane"))
        {
            Plane* pl = new Plane();
            pl->Tr = tr;

            cStr = shapeEl->Attribute("IsOneSided");
            if (cStr == nullptr)
                return Combine("Couldn't get IsOneSided attribute for plane shape, index ", iStr.c_str());
            else if (AreEqual(cStr, "true"))
                pl->IsOneSided = true;
            else
                pl->IsOneSided = false;
            
            outShapes[i] = pl;
        }
        else if (AreEqual(shapeType, "Mesh"))
        {
            std::vector<Triangle> tris;

            int nTris;
            if (shapeEl->QueryIntAttribute("NTris", &nTris) != tinyxml2::XML_NO_ERROR)
                return Combine("Couldn't get NTris attribute on mesh shape, index ", iStr.c_str());
            tris.resize(nTris);

            tinyxml2::XMLElement* triEl = shapeEl->FirstChildElement("Tri");
            while (triEl != nullptr)
            {
                Triangle t;
                int vIndex = 0;
                tinyxml2::XMLElement* vertEl = triEl->FirstChildElement();
                while (vertEl != nullptr)
                {
                    if (vIndex > 2)
                        return Combine("Too many verts in triangle in mesh shape, index ", iStr.c_str());
                    if (!AreEqual(vertEl->Name(), "Vert"))
                        return Combine("Unknown element in mesh shape's triangle, index ", iStr.c_str());
                    
                    cStr = vertEl->Attribute("Pos");
                    if (cStr == nullptr || !FromString(cStr, t.Verts[vIndex].Pos))
                        return Combine("Couldn't parse position in mesh shape, index ", iStr.c_str());
                    cStr = vertEl->Attribute("Normal");
                    if (cStr == nullptr || !FromString(cStr, t.Verts[vIndex].Normal))
                        return Combine("Couldn't parse normal in mesh shape, index ", iStr.c_str());
                    cStr = vertEl->Attribute("Tangent");
                    if (cStr == nullptr || !FromString(cStr, t.Verts[vIndex].Tangent))
                        return Combine("Couldn't parse tangent in mesh shape, index ", iStr.c_str());
                    cStr = vertEl->Attribute("Bitangent");
                    if (cStr == nullptr || !FromString(cStr, t.Verts[vIndex].Bitangent))
                        return Combine("Couldn't parse bitangent in mesh shape, index ", iStr.c_str());
                    if (vertEl->QueryAttribute("UVx", &t.Verts[vIndex].UV[0]) != tinyxml2::XML_NO_ERROR)
                        return Combine("Couldn't parse UVx in mesh shape, index ", iStr.c_str());
                    if (vertEl->QueryAttribute("UVy", &t.Verts[vIndex].UV[1]) != tinyxml2::XML_NO_ERROR)
                        return Combine("Couldn't parse UVy in mesh shape, index ", iStr.c_str());

                    vIndex += 1;
                    vertEl = vertEl->NextSiblingElement();
                }

                triEl = triEl->NextSiblingElement("Tri");
            }

            Mesh* ms = new Mesh(tris.data(), tris.size());
            ms->Tr = tr;
            outShapes[i] = ms;
        }


        //Get the material.

        tinyxml2::XMLElement* matEl = root->FirstChildElement(Combine("Material", iStr.c_str()).c_str());
        if (matEl == nullptr)
        {
            return Combine("Couldn't find material with index", iStr.c_str());
        }

        const char* matType = matEl->Attribute("Type");
        if (matType == nullptr)
            return Combine("Couldn't find type of material, index ", iStr.c_str());

        if (AreEqual(matType, "Lambert"))
        {
            Material_Lambert* ml = new Material_Lambert();

            tinyxml2::XMLElement* albedo = matEl->FirstChildElement("Albedo");
            if (albedo == nullptr || albedo->Attribute("Value") == nullptr)
                return Combine("Couldn't find Albedo/Value for material, index ", iStr.c_str());
            FromString(albedo->Attribute("Value"), ml->Color);

            outMats[i] = ml;
        }
        else if (AreEqual(matType, "Metal"))
        {
            Material_Metal* mm = new Material_Metal();
            
            tinyxml2::XMLElement* albedo = matEl->FirstChildElement("Albedo");
            if (albedo == nullptr || albedo->Attribute("Value") == nullptr)
                return Combine("Couldn't find Albedo/Value for material, index ", iStr.c_str());
            FromString(albedo->Attribute("Value"), mm->Albedo);
            
            tinyxml2::XMLElement* roughness = matEl->FirstChildElement("Albedo");
            if (roughness == nullptr ||
                roughness->QueryFloatAttribute("Value", &mm->Roughness) !=
                    tinyxml2::XML_NO_ERROR)
            {
                return Combine("Couldn't find Albedo/Value for material, index ", iStr.c_str());
            }
            
            outMats[i] = mm;
        }
        else
        {
                return Combine("Unknown material type ", matType);
        }
    }

    tinyxml2::XMLElement* skyMatEl = root->FirstChildElement("SkyMaterial");
    if (skyMatEl == nullptr)
        return "Couldn't find the SkyMaterial element";
    
    const char* cStr = skyMatEl->Attribute("Type");
    if (cStr == nullptr)
        return "SkyMaterial element needs a Type attribute";
    
    if (AreEqual(cStr, "SimpleColor"))
    {
        SkyMaterial_SimpleColor* sc = new SkyMaterial_SimpleColor();

        tinyxml2::XMLElement* colEl = skyMatEl->FirstChildElement("Color");
        if (colEl == nullptr || colEl->Attribute("Value") == nullptr ||
            !FromString(colEl->Attribute("Value"), sc->Color))
        {
            return "Error getting sky material's color";
        }

        *outSkyMat = sc;
    }
    else if (AreEqual(cStr, "VerticalGradient"))
    {
        SkyMaterial_VerticalGradient* sg = new SkyMaterial_VerticalGradient(Vector3f(), Vector3f());

        tinyxml2::XMLElement* colEl = skyMatEl->FirstChildElement("BottomColor");
        if (colEl == nullptr || colEl->Attribute("Value") == nullptr ||
            !FromString(colEl->Attribute("Value"), sg->BottomCol))
        {
            return "Error getting sky material's bottom color";
        }

        colEl = skyMatEl->FirstChildElement("TopColor");
        if (colEl == nullptr || colEl->Attribute("Value") == nullptr ||
            !FromString(colEl->Attribute("Value"), sg->TopCol))
        {
            return "Error getting sky material's top color";
        }

        *outSkyMat = sg;
    }
    else
    {
        return Combine("Unknown sky material type ", cStr);
    }

    return "";
}

std::string XmlData::ToFile(const std::string& filePath,
                            const std::vector<Shape*>& shapes,
                            const std::vector<Material*>& mats,
                            const SkyMaterial* skyMat)
{
    char str[64];
    tinyxml2::XMLDocument doc;
    tinyxml2::XMLElement* root = doc.NewElement("Scene");

    assert(shapes.size() == mats.size());
    root->SetAttribute("NShapes", shapes.size());
    

    //Shapes.
    for (unsigned int i = 0; i < shapes.size(); ++i)
    {
        Shape& shpe = *shapes[i];

        tinyxml2::XMLElement* shapeEl = doc.NewElement(Combine("Shape", ToString(i).c_str()).c_str());

        tinyxml2::XMLElement* shapeTrEl = doc.NewElement("Transform");

            ToString(str, shpe.Tr.GetPos());
            shapeTrEl->SetAttribute("Pos", str);

            ToString(str, shpe.Tr.GetScale());
            shapeTrEl->SetAttribute("Scale", str);

            Vector3f axis;
            float angle;
            shpe.Tr.GetRot().GetAxisAngle(axis, angle);
            ToString(str, axis);
            shapeTrEl->SetAttribute("RotAxis", str);
            shapeTrEl->SetAttribute("RotAngle", angle);

        shapeEl->InsertEndChild(shapeTrEl);

        if (IS_TYPE(shpe, Sphere))
        {
            shapeEl->SetAttribute("Type", "Sphere");
            Sphere& sph = dynamic_cast<Sphere&>(shpe);
        }
        else if (IS_TYPE(shpe, Plane))
        {
            shapeEl->SetAttribute("Type", "Plane");
            Plane& pln = dynamic_cast<Plane&>(shpe);

            shapeEl->SetAttribute("IsOneSided", (pln.IsOneSided ? "true" : "false"));
        }
        else if (IS_TYPE(shpe, Mesh))
        {
            shapeEl->SetAttribute("Type", "Mesh");
            Mesh& msh = dynamic_cast<Mesh&>(shpe);

            shapeEl->SetAttribute("NTris", msh.NTris);

            for (int k = 0; k < msh.NTris; ++k)
            {
                Triangle& tri = msh.Tris[k];
                tinyxml2::XMLElement* triEl = doc.NewElement("Tri");
                for (int j = 0; j < 3; ++j)
                {
                    tinyxml2::XMLElement* vertEl = doc.NewElement("Vert");

                    ToString(str, msh.Tris[k].Verts[j].Pos);
                    vertEl->SetAttribute("Pos", str);

                    ToString(str, msh.Tris[k].Verts[j].Normal);
                    vertEl->SetAttribute("Normal", str);

                    ToString(str, msh.Tris[k].Verts[j].Tangent);
                    vertEl->SetAttribute("Tangent", str);

                    ToString(str, msh.Tris[k].Verts[j].Bitangent);
                    vertEl->SetAttribute("Bitangent", str);
                            
                    vertEl->SetAttribute("UVx", msh.Tris[k].Verts[j].UV[0]);
                    vertEl->SetAttribute("UVy", msh.Tris[k].Verts[j].UV[1]);
                    triEl->InsertEndChild(vertEl);
                }
                shapeEl->InsertEndChild(triEl);
            }
        }
        else
        {
            doc.DeleteNode(shapeEl);
            doc.DeleteNode(root);
            return Combine("Unknown shape type ", typeid(shpe).name());
        }
        
        root->InsertEndChild(shapeEl);
    }


    //Materials.
    for (unsigned int i = 0; i < mats.size(); ++i)
    {
        tinyxml2::XMLElement* matEl = doc.NewElement(Combine("Material", ToString(i).c_str()).c_str());

        Material& mat = *mats[i];

        if (IS_TYPE(mat, Material_Lambert))
        {
            Material_Lambert& ml = dynamic_cast<Material_Lambert&>(mat);

            matEl->SetAttribute("Type", "Lambert");
            
            tinyxml2::XMLElement* albedo = doc.NewElement("Albedo");
            ToString(str, ml.Color);
            albedo->SetAttribute("Value", str);
            matEl->InsertEndChild(albedo);
        }
        else if (IS_TYPE(mat, Material_Metal))
        {
            Material_Metal& mm = dynamic_cast<Material_Metal&>(mat);
            
            matEl->SetAttribute("Type", "Metal");

            tinyxml2::XMLElement* albedo = doc.NewElement("Albedo");
            ToString(str, mm.Albedo);
            albedo->SetAttribute("Value", str);
            matEl->InsertEndChild(albedo);

            tinyxml2::XMLElement* roughness = doc.NewElement("Roughness");
            roughness->SetAttribute("Value", mm.Roughness);
            matEl->InsertEndChild(roughness);
        }
        else
        {
            doc.DeleteNode(matEl);
            doc.DeleteNode(root);
            return Combine("Unknown material type ", typeid(mat).name());
        }

        root->InsertEndChild(matEl);
    }


    //Sky material.
    tinyxml2::XMLElement* skyMatEl = doc.NewElement("SkyMaterial");
    if (IS_TYPE(*skyMat, SkyMaterial_SimpleColor))
    {
        const SkyMaterial_SimpleColor& smc = *dynamic_cast<const SkyMaterial_SimpleColor*>(skyMat);
        skyMatEl->SetAttribute("Type", "SimpleColor");

        tinyxml2::XMLElement* colEl = doc.NewElement("Color");
        ToString(str, smc.Color);
        colEl->SetAttribute("Value", str);
        skyMatEl->InsertEndChild(colEl);
    }
    else if (IS_TYPE(*skyMat, SkyMaterial_VerticalGradient))
    {
        const SkyMaterial_VerticalGradient& smg = *dynamic_cast<const SkyMaterial_VerticalGradient*>(skyMat);
        skyMatEl->SetAttribute("Type", "VerticalGradient");

        tinyxml2::XMLElement* colEl = doc.NewElement("BottomColor");
        ToString(str, smg.BottomCol);
        colEl->SetAttribute("Value", str);
        skyMatEl->InsertEndChild(colEl);

        colEl = doc.NewElement("TopColor");
        ToString(str, smg.TopCol);
        colEl->SetAttribute("Value", str);
        skyMatEl->InsertEndChild(colEl);
    }
    else
    {
        doc.DeleteNode(skyMatEl);
        doc.DeleteNode(root);
        return Combine("Unknown sky material type ", typeid(*skyMat).name());
    }
    root->InsertEndChild(skyMatEl);

    doc.InsertEndChild(root);

    //Save the document.
    tinyxml2::XMLError err = doc.SaveFile(filePath.c_str());
    if (err != tinyxml2::XML_NO_ERROR)
    {
        sprintf(str, "%d", (int)err);
        return Combine("Couldn't save document; tinyxml2 error code ", str);
    }

    return "";
}