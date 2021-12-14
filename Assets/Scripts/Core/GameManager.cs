using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core {
    public class GameManager : MonoBehaviour {
        [SerializeField] private int width = 4;
        [SerializeField] private int height = 4;
        [SerializeField] private Node nodePrefab;
        [SerializeField] private Block blockPrefab;
        [SerializeField] private SpriteRenderer boardPrefab;
        [SerializeField] private List<BlockType> types;
        [SerializeField] private float travelTime = 0.2f;
        [SerializeField] private int winCondition = 2048;
        [SerializeField] private GameObject mergeEffectPrefab;
        [SerializeField] private FloatingText floatingTextPrefab;

        [SerializeField] private GameObject winScreen;
        [SerializeField] private GameObject loseScreen;
        [SerializeField] private GameObject winScreenText;
        [SerializeField] private AudioClip[] moveClips;
        [SerializeField] private AudioClip[] matchClips;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;
        [SerializeField] private AudioSource source;

        private List<Node> _nodes;
        private List<Block> _blocks;
        private GameState _state;
        private int _round;

        private BlockType GetBlockTypeByValue(int value) => types.First(t => t.value == value);

        private void Start() {
            ChangeState(GameState.GenerateLevel);
        }

        private void ChangeState(GameState newState) {
            _state = newState;

            switch (newState) {
                case GameState.GenerateLevel:
                    GenerateGrid();
                    break;
                case GameState.SpawningBlocks:
                    SpawnBlocks(_round++ == 0 ? 2 : 1);
                    break;
                case GameState.WaitingInput:
                    break;
                case GameState.Moving:
                    break;
                case GameState.Win:
                    winScreen.SetActive(true);
                    source.PlayOneShot(winClip);
                    Invoke(nameof(DelayedWinScreenText), 1.5f);
                    break;
                case GameState.Lose:
                    source.PlayOneShot(loseClip);
                    loseScreen.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }

        private void DelayedWinScreenText() {
            winScreenText.SetActive(true);
        }

        private void Update() {
            if (_state != GameState.WaitingInput) return;
#if UNITY_ANDROID
            switch (SwipeManager.Direction) {
                case Swipes.None:
                    break;
                case Swipes.Up:
                    Shift(Vector2.up);
                    break;
                case Swipes.Down:
                    Shift(Vector2.down);
                    break;
                case Swipes.Left:
                    Shift(Vector2.left);
                    break;
                case Swipes.TopLeft:
                    break;
                case Swipes.BottomLeft:
                    break;
                case Swipes.Right:
                    Shift(Vector2.right);
                    break;
                case Swipes.TopRight:
                    break;
                case Swipes.BottomRight:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
#else            
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
            if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
#endif
        }
            
        private void GenerateGrid() {
            _round = 0;
            _nodes = new List<Node>();
            _blocks = new List<Block>();
            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity, transform);
                    _nodes.Add(node);
                }
            }

            var center = new Vector2(width / 2f - 0.5f, height / 2f - 0.5f);

            var board = Instantiate(boardPrefab, center, Quaternion.identity);
            board.size = new Vector2(width, height);

            Camera.main.transform.position = new Vector3(center.x, center.y, -10);

            ChangeState(GameState.SpawningBlocks);
        }

        private void SpawnBlocks(int amount) {
            var freeNodes = _nodes.Where(n => n.occupiedBlock == null).OrderBy(b => Random.value).ToList();

            foreach (var node in freeNodes.Take(amount)) {
                SpawnBlock(node, Random.value > 0.8f ? 4 : 2);
            }

            if (freeNodes.Count() == 1) {
                ChangeState(GameState.Lose);
                return;
            }

            ChangeState(_blocks.Any(b => b.value == winCondition) ? GameState.Win : GameState.WaitingInput);
        }

        private void SpawnBlock(Node node, int value) {
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(value));
            block.SetBlock(node);
            _blocks.Add(block);
        }

        private void Shift(Vector2 dir) {
            ChangeState(GameState.Moving);

            var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
            if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

            foreach (var block in orderedBlocks) {
                var next = block.node;
                do {
                    block.SetBlock(next);

                    var possibleNode = GetNodeAtPosition(next.Pos + dir);
                    if (possibleNode != null) {
                        // We know a node is present
                        // If it's possible to merge, set merge
                        if (possibleNode.occupiedBlock != null && possibleNode.occupiedBlock.CanMerge(block.value)) {
                            block.MergeBlock(possibleNode.occupiedBlock);
                        }
                        // Otherwise, can we move to this spot?
                        else if (possibleNode.occupiedBlock == null) next = possibleNode;

                        // None hit? End do while loop
                    }
                } while (next != block.node);
            }


            var sequence = DOTween.Sequence();

            foreach (var block in orderedBlocks) {
                var movePoint = block.mergingBlock != null ? block.mergingBlock.node.Pos : block.node.Pos;

                sequence.Insert(0, block.transform.DOMove(movePoint, travelTime).SetEase(Ease.InQuad));
            }

            sequence.OnComplete(
                () => {
                    var mergeBlocks = orderedBlocks.Where(b => b.mergingBlock != null).ToList();
                    foreach (var block in mergeBlocks) {
                        MergeBlocks(block.mergingBlock, block);
                    }

                    if (mergeBlocks.Any()) source.PlayOneShot(matchClips[Random.Range(0, matchClips.Length)], 0.2f);
                    ChangeState(GameState.SpawningBlocks);
                }
            );


            source.PlayOneShot(moveClips[Random.Range(0, moveClips.Length)], 0.2f);
        }

        private void MergeBlocks(Block baseBlock, Block mergingBlock) {
            var newValue = baseBlock.value * 2;

            //Instantiate(mergeEffectPrefab, baseBlock.Pos, Quaternion.identity);
            //Instantiate(floatingTextPrefab, baseBlock.Pos, Quaternion.identity).Init(newValue);

            SpawnBlock(baseBlock.node, newValue);

            RemoveBlock(baseBlock);
            RemoveBlock(mergingBlock);
        }

        private void RemoveBlock(Block block) {
            _blocks.Remove(block);
            Destroy(block.gameObject);
        }

        private Node GetNodeAtPosition(Vector2 pos) => _nodes.FirstOrDefault(n => n.Pos == pos);
    }
}