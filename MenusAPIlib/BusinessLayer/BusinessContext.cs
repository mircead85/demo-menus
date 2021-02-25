/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenusAPI;

namespace MenusAPIlib
{
    public class BusinessContext
    {
        public class BusinessData
        {
            protected WeakReference<object> DALObjectReference { get; set; }
            public object DALObject
            {
                get
                {
                    if (DALObjectReference == null)
                        return null;

                    object result;
                    if (DALObjectReference.TryGetTarget(out result))
                        return result;
                    return null;
                }
                set
                {
                    if (value == null)
                        DALObjectReference = null;

                    DALObjectReference = new WeakReference<object>(value);
                }
            }

            public int? DALObjectID { get; set; }
            public BusinessData() { }
        }

        protected int _NextID = 1;
        protected Dictionary<int, Tuple<BusinessData, WeakReference<BusinessObject>>> BusinessID2Data = new Dictionary<int, Tuple<BusinessData, WeakReference<BusinessObject>>>();

        public List<BusinessObject> DeletedObjects { get; internal set; }

        public bool RegisterBusinessObject(BusinessObject objToRegister)
        {
            var objID = objToRegister.ObjectID.HasValue ? objToRegister.ObjectID.Value : 0;
            if (objID != 0)
                if (ValidateID(objID, objToRegister))
                    return false;

            objID = _NextID++;

            BusinessID2Data[objID] = new Tuple<BusinessData,WeakReference<BusinessObject>>(
                                                                new BusinessData(), 
                                                                new WeakReference<BusinessObject>(objToRegister));
            objToRegister.ObjectID = objID;

            return true;
        }

        protected bool ValidateID(int objID, BusinessObject obj)
        {
            Tuple<BusinessData, WeakReference<BusinessObject>> entry;
            BusinessID2Data.TryGetValue(objID, out entry);
            if (entry == null)
                return false;
            BusinessObject targetObj;
            if (entry.Item2.TryGetTarget(out targetObj))
                if (targetObj == obj)
                    return true;
                else
                {
                    BusinessID2Data.Remove(objID); //Clean the dictionary
                    return false;
                }
            return false;
        }

        public bool UnregisterBusinesObject(BusinessObject objToUnregister)
        {
            if (objToUnregister == null)
                return false;

            if (!objToUnregister.ObjectID.HasValue)
                return false;

            var objID = objToUnregister.ObjectID.Value;
            if (!ValidateID(objID, objToUnregister))
                return false;

            BusinessID2Data.Remove(objID);
            return true;
        }

        public BusinessData this[BusinessObject obj, bool bRegisterIfNotPresent = false]
        {
            get
            {
                if (obj == null)
                    return null;

                int objId = 0;
                if (!obj.ObjectID.HasValue)
                    if (!bRegisterIfNotPresent)
                        return null;
                    else
                        objId = 0; //Not present as ID.
                else
                    objId = obj.ObjectID.Value;

                if (bRegisterIfNotPresent)
                {
                    if (!ValidateID(objId, obj))
                        RegisterBusinessObject(obj);
                    objId = obj.ObjectID.Value;
                }

                if (!ValidateID(objId, obj))
                    return null;

                return BusinessID2Data[objId].Item1;
            }
        }


        public List<BusinessObject> GenerateNewlyCreatedObjectsList()
        {
            var result = new List<BusinessObject>();

            foreach (var entry in BusinessID2Data.Values)
            {
                BusinessObject oldBO;
                if (entry.Item2.TryGetTarget(out oldBO))
                    if (oldBO.IsNew)
                        if (entry.Item1.DALObject != null)
                        {
                            var newBO = APILibUtils.CreateBusinessObjectFrom(entry.Item1.DALObject);
                            if (newBO != null)
                            {
                                newBO.CorrelationId = oldBO.CorrelationId;
                                result.Add(newBO);
                            }
                        }
            }

            return result;
        }

        public APIInboundRequest OriginalRequest { get; protected set; }

        public BusinessContext(APIInboundRequest request)
        {
            OriginalRequest = request;
            DeletedObjects = new List<BusinessObject>();
        }
    }
}
