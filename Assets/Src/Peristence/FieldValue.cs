using System.Diagnostics;

namespace Weichx.Persistence {

    [DebuggerDisplay("value = {value}")]
    public struct FieldValue {

        public readonly object value;
        public readonly TypeDefinition typeDefinition;

        public FieldValue(TypeDefinition typeDefinition, object value) {
            this.typeDefinition = typeDefinition;
            this.value = value;
        }

    }

}