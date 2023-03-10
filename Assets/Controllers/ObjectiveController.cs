using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class ObjectiveController : MonoBehaviour
{
    public event Action OnRequestNewRecipe;
    public event Action OnRequestPoisonRecipe;
    public event Action OnRequestNewCategories;
    public event Action<int> OnRequestIntroDialog;
    public event Action<int> OnRequestFirstIngredientDialog;
    public event Action<int> OnRequestCorrectIngredientDialog;
    public event Action<int> OnRequestWrongIngredientDialog;

    public event Action<int> OnLivesCountChanged;

    private RecipeData currentRecipe;
    private RecipeData poisonRecipe;
    private int lives = 3;
    private int level = 1;

    private bool lastRecipeValid;
    private bool potionProvided;
    private bool poisonProvided;
    private bool firstValidIngredientFound;

    private void Start()
    {
        StartCoroutine(GameLoop());
    }

    public void SetCurrentRecipe(RecipeData recipe)
    {
        currentRecipe = recipe;
    }

    public void SetPoisonRecipe(RecipeData recipe)
    {
        poisonRecipe = recipe;
    }

    public bool EvaluateRecipe(HashSet<ItemData> ingredients)
    {
        if (level == 5)
        {
            poisonProvided = poisonRecipe.ingredients.TrueForAll((x) =>
            ingredients.Contains(x));
        }

        lastRecipeValid = currentRecipe.ingredients.TrueForAll((x) =>
            ingredients.Contains(x));

        potionProvided = true;

        return lastRecipeValid;
    }

    public int GetLives()
    {
        return lives;
    }

    public int GetLevel()
    {
        return level;
    }

    public void NotifyIngredientCollected(ItemData item)
    {
        if (firstValidIngredientFound) return;

        firstValidIngredientFound = true;
        OnRequestFirstIngredientDialog?.Invoke(level-1);
    }

    private IEnumerator GameLoop()
    {
        Debug.Log("[GameLoop]: Requesting poison recipe");
        OnRequestPoisonRecipe?.Invoke();

        while (true)
        {
            Debug.Log("[GameLoop]: Requesting recipe");
            OnRequestNewRecipe?.Invoke();
            OnRequestIntroDialog?.Invoke(level-1);

            lastRecipeValid = false;
            potionProvided = false;
            poisonProvided = false;
            firstValidIngredientFound = false;

            yield return new WaitUntil(() => potionProvided);

            Debug.Log("[GameLoop]: Evaluating recipe");

            // All levels
            if (level < 5)
            {
                if (lastRecipeValid)
                {
                    Debug.Log("OnRequestCorrectIngredientDialog Invoke");
                    OnRequestCorrectIngredientDialog?.Invoke(level-1);
                    yield return SleepCutscene();
                }

                else
                {
                    OnRequestWrongIngredientDialog?.Invoke(level-1);
                    --lives;
                    OnLivesCountChanged?.Invoke(lives);
                    if (lives < 1)
                    {
                        
                        StartCoroutine(EarlyGameOver());
                        yield break;
                    }
                }
            }
            // Last level
            else
            {
                if(poisonProvided)
                {
                    StartCoroutine(PoisonCutscene());
                    yield break;
                }

                lives = 0;
                OnLivesCountChanged?.Invoke(lives);

                // You fail even if the recipe is "valid"
                StartCoroutine(GameOver());
                yield break;
            }

            OnRequestNewCategories?.Invoke();
            level++;
        }
    }

    private IEnumerator PoisonCutscene()
    {
        Debug.Log("[GameLoop]: You poisoned the witch!");
        SceneManager.LoadScene("VictoryScreen");
        yield return null;
    }

    private IEnumerator SleepCutscene()
    {
        Debug.Log("[GameLoop]: The witch fell asleep...");
        yield return null;
    }

    private IEnumerator EarlyGameOver()
    {
        Debug.Log("[GameLoop]: The witch got angry and ate you!");
        SceneManager.LoadScene("DefeatScreen");
        yield return null;
    }

    private IEnumerator GameOver()
    {
        Debug.Log("[GameLoop]: You did well, but the witch still ate you!");
        SceneManager.LoadScene("DefeatScreen");
        yield return null;
    }
}
