public static class HelperMethods
{
    public static int _id = 0;
    public static int GetNextId() { int id = _id; _id++; return id; }
    public static void ResetIds() { _id = 0; }
}


/*
        iter shapes
            dict<obj, id>
                HelperMethod.GetId(obj) - stores
            shape
                List<int> ids

            Load
                if parentId == -1 (invalid) can just spawn in then insert to dictionary<int, shapeScript>
                if parent != -1 (valid) check if we have a parent
                    if we have a parent instantiate the object and child it to the parent -> then update data
                    if we dont have a parent instantiate the object
                        when we instantiate the parent, we need to child to the parent but keeping the scale rotation, etc. the same
                            -> We should store the world coordinates this reason
       
 */