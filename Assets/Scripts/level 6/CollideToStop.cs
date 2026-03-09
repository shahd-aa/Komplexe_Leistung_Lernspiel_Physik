using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CollideToStop : MonoBehaviour
{
    public GameManager_Lvl_06 gameManager;
    public Rigidbody rbRope;
    public Animator animatorTeam1;

    void OnCollisionEnter(Collision collision)
    {
        gameManager.StopPulling();

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 hitPoint = contact.point;

            // Loop through the teams
            foreach (Transform team in rbRope.transform)
            {
                foreach (Transform character in team)
                {
                    Collider charCollider = character.GetComponent<Collider>();
                    if (charCollider != null && charCollider.bounds.Contains(hitPoint))
                    {
                        if (team.CompareTag("Team1"))
                        {
                            Debug.Log("Setting bool now");
                            animatorTeam1.applyRootMotion = true;
                            animatorTeam1.SetBool("hasTripped", true);
                            StartCoroutine(LerpCharacterDown(character, 0.5f, 1.75f, 3f));
                            gameManager.DetermineWinner(2);
                            Debug.Log("team 2 won!");
                            return;
                        }
                    }
                }
            }
        }
        // No character hit => tie
        StartCoroutine(ShowTie());
    }

    IEnumerator ShowTie()
    {
        Debug.Log("its a tie, showing ui soon");
        yield return new WaitForSeconds(3f);
    }

    IEnumerator LerpCharacterDown(Transform character, float duration, float xValue, float yValue)
    {
        Vector3 startPos = character.position;
        Vector3 endPos = new Vector3(startPos.x - xValue, startPos.y - yValue, startPos.z);

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;

            character.position = Vector3.Lerp(startPos, endPos, normalized);

            yield return null;
        }

        character.position = endPos; // snap to final position
    }
}
