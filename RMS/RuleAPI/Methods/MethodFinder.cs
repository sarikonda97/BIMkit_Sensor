using DbmsApi;
using DbmsApi.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RuleAPI.Methods
{
    public static class MethodFinder
    {
        public static readonly double SchrinkAmount = 1.0;

        public static List<ObjectType> GetAllTypes()
        {
            return ObjectTypeTree.GetAllTypes();
        }

        public static Dictionary<string, Type> GetAllPropertyMethods()
        {
            MethodInfo[] methodInfos = typeof(PropertyMethods).GetMethods(BindingFlags.Public | BindingFlags.Static);

            Dictionary<string, Type> methodNames = new Dictionary<string, Type>();
            foreach (var methodInfo in methodInfos)
            {
                methodNames.Add(methodInfo.Name, methodInfo.ReturnType);
            }
            return methodNames;
        }

        public static Dictionary<string, Type> GetAllRelationMethods()
        {
            MethodInfo[] methodInfos = typeof(RelationMethods).GetMethods(BindingFlags.Public | BindingFlags.Static);

            Dictionary<string, Type> methodNames = new Dictionary<string, Type>();
            foreach (var methodInfo in methodInfos)
            {
                methodNames.Add(methodInfo.Name, methodInfo.ReturnType);
            }
            return methodNames;
        }

        public static Dictionary<string, ObjectType> GetAllVOMethods()
        {
            MethodInfo[] methodInfos = typeof(VirtualObjects).GetMethods(BindingFlags.Public | BindingFlags.Static);

            Dictionary<string, ObjectType> methodNames = new Dictionary<string, ObjectType>();
            foreach (MethodInfo methodInfo in methodInfos)
            {
                ObjectType methodVOType = GetAllTypes().Where(e => e.Name.Contains(methodInfo.Name)).First();
                methodNames.Add(methodInfo.Name, methodVOType);
            }
            return methodNames;
        }

        public static Dictionary<string, MethodInfo> GetAllVOMethodInfos()
        {
            MethodInfo[] methodInfos = typeof(VirtualObjects).GetMethods(BindingFlags.Public | BindingFlags.Static);

            Dictionary<string, MethodInfo> methodInfoDict = new Dictionary<string, MethodInfo>();
            foreach (var methodInfo in methodInfos)
            {
                //ObjectType methodVOType = GetAllTypes().Where(e => e.Name.Contains(methodInfo.Name)).First();
                methodInfoDict.Add(methodInfo.Name, methodInfo);
            }
            return methodInfoDict;
        }

        public static Dictionary<string, MethodInfo> GetAllPropertyInfos()
        {
            MethodInfo[] methodInfos = typeof(PropertyMethods).GetMethods(BindingFlags.Public | BindingFlags.Static);

            Dictionary<string, MethodInfo> methodInfoDict = new Dictionary<string, MethodInfo>();
            foreach (var methodInfo in methodInfos)
            {
                methodInfoDict.Add(methodInfo.Name, methodInfo);
            }
            return methodInfoDict;
        }

        public static Dictionary<string, MethodInfo> GetAllRelationInfos()
        {
            MethodInfo[] methodInfos = typeof(RelationMethods).GetMethods(BindingFlags.Public | BindingFlags.Static);

            Dictionary<string, MethodInfo> methodInfoDict = new Dictionary<string, MethodInfo>();
            foreach (var methodInfo in methodInfos)
            {
                methodInfoDict.Add(methodInfo.Name, methodInfo);
            }
            return methodInfoDict;
        }
    }
}