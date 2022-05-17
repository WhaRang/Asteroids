using UnityEngine;
using UnityEngine.UI;

public class GameStateController : MonoBehaviour
{
    [SerializeField] private Image restartPanel;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ParticleSelfCollisionSystem particleSelfCollisionSystem;
    [SerializeField] private ShipController ship;

    public void RestartGame()
    {
        ship.Restart();
        scoreManager.ResetScore();
        ship.gameObject.SetActive(true);
        restartPanel.gameObject.SetActive(false);
        particleSelfCollisionSystem.Restart();
    }

    public void StopGame()
    {
        ship.gameObject.SetActive(false);
        restartPanel.gameObject.SetActive(true);
    }
}
