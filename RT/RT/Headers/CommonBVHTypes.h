#pragma once

#include "Root.h"
#include "Bounds.h"

#include "Vectors.h"


//Defines common types of BVH that use axis-aligned bounding boxes/cubes.
namespace BVH
{
    template<typename T, typename BoundsFactory>
    using TwoD_AABB = Root<T, AABounds<RT::Vector2f>, BoundsFactory>;

    template<typename T, typename BoundsFactory>
    using ThreeD_AABB = Root<T, AABounds<RT::Vector3f>, BoundsFactory>;
}