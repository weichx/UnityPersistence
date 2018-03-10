using System.Diagnostics;

namespace Weichx.Persistence {

    [DebuggerDisplay("fieldName = {fieldName}")]
    public struct FieldDefinition {

        public readonly string fieldName;
        public readonly TypeDefinition typeDefinition;

        public FieldDefinition(string fieldName, TypeDefinition typeDefinition) {
            this.fieldName = fieldName;
            this.typeDefinition = typeDefinition;
        }

    }

}