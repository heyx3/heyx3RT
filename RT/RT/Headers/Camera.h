#pragma once

#include "Matrix4f.h"
#include "DataSerialization.h"


//Has two modes, indicated by the "LockUp" flag:
//   - If "LockUp" is true, the camera's Up vector will not be affected by pitching,
//        and the Forward vector can only get so close to it (specified by "ClosestDotVariance").
//   - If "LockUp" is false, the camera's Up vector is not locked in any way.
//Note that rolling this camera or rotating it with a Quaternion will change the Up vector
//    regardless of "LockUp".
class RT_API Camera : public ISerializable
{
public:

    Vector3f Pos;

    //If true, keeps the upward vector along the positive Z axis regardless of rotation.
    //Note that rotating this camera with a quaternion will rotate the up vector regardless of this field.
    bool LockUp;
    //If "LockUp" is true, this field indicates the closest the camera can get to the up/down vector.
    //Should be between 0 (can look all the way up/down) and 1 (can't pitch the camera at all).
    float ClosestDotVariance;

    //Aspect ratio.
    float WidthOverHeight;

    
    Camera(const Vector3f& pos, const Vector3f& forward, const Vector3f& upward,
           float widthOverHeight, bool lockUp = true, float closestDotVariance = 0.05f);


	Vector3f GetForward(void) const { return forward; }
	Vector3f GetUpward(void) const { return up; }
	Vector3f GetSideways(void) const { return sideways; }

	void SetRotation(const Vector3f& newForward, const Vector3f& newUp);
    void Rotate(const Quaternion& rotation);
	void AddPitch(float radians);
	void AddYaw(float radians);
	void AddRoll(float radians);


    virtual void ReadData(DataReader& data) override;
    virtual void WriteData(DataWriter& data) const override;
	

private:

	Vector3f forward, up, sideways;

    void UpdateSideways() { sideways = forward.Cross(up); }
};
