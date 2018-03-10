using System;
using System.Diagnostics;
using System.Reflection;

namespace Weichx.Persistence {

    [DebuggerDisplay("fieldName = {fieldDefinition.fieldName}")]
    public class SnapshotNode {

        public FieldValue fieldValue;
        public FieldDefinition fieldDefinition;

        public SnapshotNode parent;
        public SnapshotNode[] children;

        public SnapshotNode(SnapshotNode parent, FieldDefinition fieldDefinition, FieldValue fieldValue) {
            this.parent = parent;
            this.fieldDefinition = fieldDefinition;
            this.fieldValue = fieldValue;
            CreateChildren();
        }

        public bool SetValue(object value) {
            TypeDefinition fvDef = value != null ? TypeDefinition.Get(value.GetType()) : fieldDefinition.typeDefinition;
            if (fvDef.type.IsAssignableFrom(fieldDefinition.typeDefinition.type)) {
                this.fieldValue = new FieldValue(fvDef, value);
                return true;
            }
            return false;
        }

        public void CreateChildren() {
            TypeDefinition typeDefinition = GetTypeDefinition();

            if (typeDefinition.IsPrimitiveLike || typeDefinition.IsType) {
                children = new SnapshotNode[0];
            }
            else if (typeDefinition.IsArray) {
                Array array = fieldValue.value as Array;
                Debug.Assert(array != null, nameof(array) + " != null");
                children = new SnapshotNode[array.Length];
                TypeDefinition elementTypeDef = TypeDefinition.Get(typeDefinition.type.GetElementType());
                for (int i = 0; i < array.Length; i++) {
                    FieldDefinition fd = new FieldDefinition(i.ToString(), elementTypeDef);
                    object memberValue = array.GetValue(i);
                    TypeDefinition memberTypeDef = memberValue == null ? elementTypeDef : TypeDefinition.Get(memberValue.GetType());
                    FieldValue fv = new FieldValue(memberTypeDef, memberValue);
                    children[i] = new SnapshotNode(this, fd, fv);
                }
            }
            else {
                FieldInfo[] fields = typeDefinition.fields;
                children = new SnapshotNode[fields.Length];
                for (int i = 0; i < fields.Length; i++) {
                    FieldInfo fi = fields[i];
                    object value = fieldValue.value != null ? fi.GetValue(fieldValue.value) : null;
                    TypeDefinition declaredType = TypeDefinition.Get(fi.FieldType);
                    TypeDefinition fieldTypeDef = value == null ? declaredType : TypeDefinition.Get(value.GetType());

                    FieldValue fv = new FieldValue(fieldTypeDef, value);
                    FieldDefinition fd = new FieldDefinition(fi.Name, declaredType);
                    children[i] = new SnapshotNode(this, fd, fv);
                }
            }
        }

        public TypeDefinition GetTypeDefinition() {
            return fieldValue.value == null ? fieldDefinition.typeDefinition : fieldValue.typeDefinition;
        }

        public string Serialize() {
            return new StringSerializer(this).Serialize();
        }

        public object Deserialize() {
            TypeDefinition typeDefinition = GetTypeDefinition();
            if (typeDefinition.IsPrimitiveLike || typeDefinition.IsType) {
                return fieldValue.value;
            }
            else if (typeDefinition.IsArray) {
                Type typeElement = typeDefinition.type.GetElementType();
                Array retn = Array.CreateInstance(typeElement, children.Length);
                for (int i = 0; i < retn.Length; i++) {
                    retn.SetValue(children[i].Deserialize(), i);
                }
                return retn;
            }
            else {
                object target = Activator.CreateInstance(typeDefinition.type);
                FieldInfo[] fields = typeDefinition.fields;
                for (int i = 0; i < fields.Length; i++) {
                    fields[i].SetValue(target, children[i].Deserialize());
                }
                return target;
            }
        }

    }

}