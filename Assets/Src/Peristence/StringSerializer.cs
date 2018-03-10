using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Weichx.Persistence {

    public class StringSerializer {

        private const char StructChar = '&';
        private const char ReferenceChar = '%';
        private const char PrimitiveChar = '-';
        private const char TypeChar = '*';
        private const char UnityChar = '!';
        private const char UnknownChar = '?';

        private SnapshotNode root;
        private Dictionary<Type, int> typeMap;
        private Dictionary<Assembly, int> assemblyMap;
        private Dictionary<object, RefIdTypeIdNode> referenceMap;

        private int typeIdGenerator;
        private int assemblyIdGenerator;
        private int referenceIdGenerator;

        private StringBuilder types;
        private StringBuilder fields;
        private StringBuilder assemblies;
        private StringBuilder references;

        public StringSerializer(SnapshotNode root) {
            this.root = root;
            this.typeMap = new Dictionary<Type, int>();
            this.assemblyMap = new Dictionary<Assembly, int>();
            this.referenceMap = new Dictionary<object, RefIdTypeIdNode>();
            this.types = new StringBuilder(500);
            this.fields = new StringBuilder(500);
            this.assemblies = new StringBuilder(500);
            this.references = new StringBuilder(500);
        }

        private struct RefIdTypeIdNode {

            public int refId;
            public int typeId;
            public SnapshotNode node;

            public RefIdTypeIdNode(int refId, int typeId, SnapshotNode child) {
                this.refId = refId;
                this.typeId = typeId;
                this.node = child;
            }

        }

        private string WriteAssemblyLine(int assemblyId, string assemblyName) {
            return $"@A{assemblyId} {assemblyName}";
        }

        private string WriteTypeLine(int typeId, int assemblyId, string typeName) {
            return $"@T{typeId} @A{assemblyId} {typeName}";
        }

        private string WriteReferenceSectionLine(int refId, int typeId, int fieldCount) {
            return $"@R{refId} @T{typeId} @C{fieldCount}";
        }

        private string WriteReferenceFieldDefinitionLine(int refId, int typeId, int fieldCount) {
            return $"^ @R{refId} @T{typeId} @C{fieldCount}";
        }

        private string WriteReferenceFieldLine(int refId, string name) {
            return $"{ReferenceChar} @R{refId} {name}";
        }

        private string WriteUnityFieldLine(int typeId, string name) {
            return $"{UnityChar} @T{typeId} {name}"; // todo -- handle this case
        }

        private string WritePrimitiveFieldLine(SnapshotNode node) {
            return StringifyPrimitive(node);
        }

        private string WriteStructLine(int typeId, string name, int fieldCount) {
            return $"{StructChar} @T{typeId} {name} @C{fieldCount}";
        }

        private string WriteTypeFieldLine(int typeId, string name) {
            return $"{TypeChar} @T{typeId} {name}";
        }

        private string WriteUnknownFieldLine(int typeId, string name) {
            return $"{UnknownChar} @T{typeId} {name}";
        }

        public string Serialize() {
            referenceMap[root.fieldValue.value] = new RefIdTypeIdNode(
                referenceIdGenerator++,
                GetTypeId(root.GetTypeDefinition()),
                root);
            FindReferences(root);
            foreach (KeyValuePair<object, RefIdTypeIdNode> kvp in referenceMap) {
                int refId = kvp.Value.refId;
                int typeId = kvp.Value.typeId;
                int fieldCount = kvp.Value.node.children.Length;
                references.AppendLine(WriteReferenceSectionLine(refId, typeId, fieldCount));
                fields.AppendLine(WriteReferenceFieldDefinitionLine(refId, typeId, fieldCount));
                WriteFields(kvp.Value.node);
            }
            assemblies.AppendLine("");
            assemblies.AppendLine(types.ToString());
            assemblies.AppendLine(references.ToString());
            assemblies.Append(fields);
            return assemblies.ToString();
        }

        private void WriteFields(SnapshotNode node) {
            for (int i = 0; i < node.children.Length; i++) {
                SnapshotNode child = node.children[i];
                TypeDefinition childTypeDefinition = child.GetTypeDefinition();
                int typeId = GetTypeId(childTypeDefinition);
                int fieldCount = child.children.Length;
                string fieldName = child.fieldDefinition.fieldName;

                if (childTypeDefinition.IsPrimitiveLike) {
                    fields.AppendLine(WritePrimitiveFieldLine(child));
                }
                else if (childTypeDefinition.IsStructType) {
                    fields.AppendLine(WriteStructLine(typeId, fieldName, fieldCount));
                    WriteFields(child);
                }
                else if (childTypeDefinition.IsArray) {
                    fields.AppendLine(WriteReferenceFieldLine(referenceMap[child.fieldValue.value].refId, fieldName));
                }
                else if (childTypeDefinition.IsReferenceType) {
                    fields.AppendLine(WriteReferenceFieldLine(referenceMap[child.fieldValue.value].refId, fieldName));
                }
                else if (childTypeDefinition.IsType) {
                    Type typeValue = child.fieldValue.value as Type;
                    if (typeValue != null) {
                        int typeValueId = GetTypeId(TypeDefinition.Get(typeValue));
                        fields.AppendLine(WriteTypeFieldLine(typeValueId, fieldName));
                    }
                    else {
                        fields.AppendLine(WriteTypeFieldLine(-1, fieldName));
                    }
                }
                else if (childTypeDefinition.IsUnityType) {
                    fields.AppendLine(WriteUnityFieldLine(typeId, fieldName));
                }
                else {
                    fields.AppendLine(WriteUnknownFieldLine(typeId, fieldName));
                }
            }
        }

        private void FindReferences(SnapshotNode node) {
            SnapshotNode[] children = node.children;
            if (children == null || children.Length == 0) return;
            for (int i = 0; i < children.Length; i++) {
                SnapshotNode child = children[i];
                TypeDefinition childTypeDefinition = child.GetTypeDefinition();
                if (childTypeDefinition.IsArray || childTypeDefinition.IsReferenceType) {
                    if (referenceMap.ContainsKey(child.fieldValue.value)) {
                        continue;
                    }
                    referenceMap[child.fieldValue.value] = new RefIdTypeIdNode(
                        referenceIdGenerator++,
                        GetTypeId(childTypeDefinition),
                        child);
                    FindReferences(child);
                }
                else if (childTypeDefinition.IsStructType) {
                    FindReferences(child);
                }
            }
        }

        protected int GetTypeId(TypeDefinition typeDefinition) {
            int typeId;
            if (!typeMap.TryGetValue(typeDefinition.type, out typeId)) {

                if (!assemblyMap.ContainsKey(typeDefinition.type.Assembly)) {
                    int assemblyId = assemblyIdGenerator++;
                    assemblyMap.Add(typeDefinition.type.Assembly, assemblyId);
                    assemblies.AppendLine(WriteAssemblyLine(assemblyId, typeDefinition.type.Assembly.FullName));
                }

                typeId = typeIdGenerator++;
                typeMap.Add(typeDefinition.type, typeId);
                types.AppendLine(WriteTypeLine(typeId, assemblyMap[typeDefinition.type.Assembly], typeDefinition.type.FullName));
            }
            return typeId;

        }

        public string StringifyPrimitive(SnapshotNode node) {
            object value = node.fieldValue.value;
            string fieldName = node.fieldDefinition.fieldName;
            TypeDefinition td = node.GetTypeDefinition();
            int typeId = GetTypeId(td);
            switch (td.typeValue) {
                case TypeValue.Double:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {(double) value:R}";
                case TypeValue.Decimal:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {Convert.ToDouble(value):R}";
                case TypeValue.Float:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {(float) value:R}";
                case TypeValue.Enum:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {(int) value}";
                case TypeValue.String:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {Regex.Escape(value.ToString())}";
                case TypeValue.Boolean:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value.ToString().ToLower()}";
                case TypeValue.Integer:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.UnsignedInteger:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.SignedByte:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.Byte:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.Short:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.UnsignedShort:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.Long:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.UnsignedLong:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
                case TypeValue.Char:
                    return $"{PrimitiveChar} @T{typeId} {fieldName} {value}";
//                case TypeValue.Vector2:
//                    Vector2 v2 = (Vector2) value;
//                    return $"{StructChar} @T{typeId} {fieldName} {v2.x:R} {v2.y:R}";
//                case TypeValue.Vector3:
//                    Vector3 v3 = (Vector3) value;
//                    return $"{StructChar} @T{typeId} {fieldName} {v3.x:R} {v3.y:R} {v3.z:R}";
//                case TypeValue.Vector4:
//                    Vector4 v4 = (Vector4) value;
//                    return $"{StructChar} @T{typeId} {fieldName} {v4.x:R} {v4.y:R} {v4.z:R} {v4.w:R}";
//                case TypeValue.Quaternion:
//                    Quaternion q = (Quaternion) value;
//                    return $"{StructChar} @T{typeId} {fieldName} {q.x:R} {q.y:R} {q.z:R} {q.w:R}";
//                case TypeValue.Color:
//                    Color c = (Color) value;
//                    return $"{StructChar} @T{typeId} {fieldName} {c.r:R} {c.g:R} {c.b:R} {c.a:R}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}