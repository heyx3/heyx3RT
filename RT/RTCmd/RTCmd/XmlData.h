#pragma once

#include <Tracer.h>


//All functions return an error string, or the empty string if everything went fine.
class XmlData
{
public:

    static std::string FromFile(const std::string& filePath,
                                std::vector<Shape*>& outShapes,
                                std::vector<Material*>& outMats,
                                SkyMaterial** outSkyMat);
    static std::string ToFile(const std::string& filePath,
                              const std::vector<Shape*>& shapes,
                              const std::vector<Material*>& mats,
                              const SkyMaterial* skyMat);
};