using UnityEngine;

public class Chapter2PuzzleTrigger : MonoBehaviour
{
    [SerializeField] private ItemsPuzzleChapter2 items;

    private void OnTriggerEnter(Collider other)
    {
        foreach (ItemsToPlace item in items.AllItemsPuzzle)
        {
            if (other.gameObject.name == item.name)
            {
                Destroy(other.gameObject);
                item.obj.transform.SetPositionAndRotation(transform.position, transform.rotation);

                Debug.Log($"Item '{item.name}' placed at trigger '{gameObject.name}'.");
                FindAnyObjectByType<Chapter2PuzzleManager>().RegisterItemPlaced();

                Destroy(this); 
                break;
            }
        }
    }
}