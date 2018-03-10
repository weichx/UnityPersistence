
namespace Weichx.Persistence {

    public class Snapshot<T> {

        private SnapshotNode rootNode;

        public Snapshot(T target) {

            TypeDefinition declaredType = TypeDefinition.Get(typeof(T));
            TypeDefinition fieldTypeDef = target == null ? declaredType : TypeDefinition.Get(target.GetType());

            FieldValue fv = new FieldValue(fieldTypeDef, target);
            FieldDefinition fd = new FieldDefinition("--Root--", declaredType);
            rootNode = new SnapshotNode(null, fd, fv);

        }

        public string Serialize() {
            return rootNode.Serialize();
        }

        public T Deserialize() {
            return (T) rootNode.Deserialize();
        }

        public static Snapshot<T> FromString(string serializedSnapshot) {
            return new Snapshot<T>(new StringDeserializer().ObjectFromString<T>(serializedSnapshot));
        }

    }

}