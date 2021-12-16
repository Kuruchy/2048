using TMPro;
using UnityEngine;

namespace Core {
    public class Block : MonoBehaviour {
        public int value;
        public Node node;
        public Block mergingBlock;
        public bool merging;
        public Vector2 Pos => transform.position;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer shadowSpriteRenderer;
        [SerializeField] private TextMeshPro text;
        public void Init(BlockType type) {
            value = type.value;
            spriteRenderer.color = type.color;
            shadowSpriteRenderer.color = type.shadow;
            text.text = type.value.ToString();
        }

        public void SetBlock(Node node) {
            if (this.node != null) this.node.occupiedBlock = null;
            this.node = node;
            this.node.occupiedBlock = this;
        }

        public void MergeBlock(Block blockToMergeWith) {
            // Set the block we are merging with
            mergingBlock = blockToMergeWith;

            // Set current node as unoccupied to allow blocks to use it
            node.occupiedBlock = null;

            // Set the base block as merging, so it does not get used twice.
            blockToMergeWith.merging = true;
        }

        public bool CanMerge(int value) => value == this.value && !merging && mergingBlock == null;
    }
}
