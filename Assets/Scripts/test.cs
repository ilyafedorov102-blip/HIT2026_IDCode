using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class test : MonoBehaviour
{
    // Типы действий при нажатии правильной кнопки
    public enum ActionType
    {
        None,           // без действия
        ShowObject,     // появление объекта
        HideObject,     // удаление/скрытие объекта
        MoveObject,     // перемещение объекта
        ChangeImage,    // изменение изображения
        ChangeScene
    }

    [System.Serializable]
    public class Step
    {
        [TextArea] public string instructionText;  // текст для отображения на шаге
        public string requiredButtonName;          // имя кнопки, которую нужно нажать

        // Настройки действия
        public ActionType actionType = ActionType.None;
        public GameObject targetObject;             // целевой объект для действия
        public Vector3 moveTargetPosition;          // позиция для перемещения
        public Sprite newImageSprite;               // новое изображение (для ChangeImage)
        public float moveSpeed = 5f;                // скорость перемещения
    }
    public List<GameObject> Hidden_objects;
    public List<Step> steps;               // список всех шагов
    public TextMeshProUGUI displayText;    // ссылка на TMPro Text
    public Button[] allButtons;             // все кнопки, которые могут участвовать
    public string final_scene;

    private int currentStepIndex = 0;
    private bool waitingForCorrectButton = false;
    private bool isMoving = false;           // флаг для анимации перемещения
    private Step currentMovingStep;          // текущий шаг с перемещением

    void Start()
    {
        if (steps.Count == 0)
        {
            Debug.LogError("Нет шагов!");
            return;
        }

        // Подписываем все кнопки на обработчик
        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() => OnAnyButtonClick(btn));
        }
        foreach (GameObject hidobj in Hidden_objects)
        {
            hidobj.SetActive(false);
        }
        StartStep(0);
    }

    void Update()
    {
        // Обработка анимации перемещения
        if (isMoving && currentMovingStep != null && currentMovingStep.targetObject != null)
        {
            MoveObjectSmoothly(currentMovingStep);
        }
    }

    void StartStep(int index)
    {
        if (index >= steps.Count)
        {
            SceneManager.LoadScene(final_scene);
            return;
        }

        Step step = steps[index];
        displayText.text = step.instructionText;
        waitingForCorrectButton = true;
        currentStepIndex = index;

        Debug.Log($"Шаг {index}: ждём кнопку {step.requiredButtonName}");
    }

    void OnAnyButtonClick(Button clickedButton)
    {
        if (!waitingForCorrectButton) return;

        Step currentStep = steps[currentStepIndex];

        // Проверяем, та ли кнопка нажата
        if (clickedButton.name == currentStep.requiredButtonName)
        {
            Debug.Log($"✅ Верно! Переход со шага {currentStepIndex}");

            // Выполняем действие перед переходом на следующий шаг
            ExecuteAction(currentStep);

            waitingForCorrectButton = false;
            StartStep(currentStepIndex + 1);
        }
        else
        {
            Debug.Log($"❌ Не та кнопка: {clickedButton.name}, нужно {currentStep.requiredButtonName}");
            // Можно добавить звук ошибки или мигание текста
        }
    }

    void ExecuteAction(Step step)
    {
        if (step.targetObject == null)
        {
            Debug.LogWarning($"Нет целевого объекта для действия на шаге {currentStepIndex}");
            return;
        }

        switch (step.actionType)
        {
            case ActionType.ShowObject:
                ShowObject(step.targetObject);
                break;

            case ActionType.HideObject:
                HideObject(step.targetObject);
                break;

            case ActionType.MoveObject:
                StartMovingObject(step);
                break;

            case ActionType.ChangeImage:
                ChangeObjectImage(step);
                break;

            case ActionType.None:
                Debug.Log("Действие не назначено");
                break;
        }
    }

    // 1. Появление объекта
    void ShowObject(GameObject obj)
    {
        obj.SetActive(true);
        Debug.Log($"Объект {obj.name} появился");
    }

    // 2. Удаление/скрытие объекта
    void HideObject(GameObject obj)
    {
        obj.SetActive(false);
        Debug.Log($"Объект {obj.name} скрыт");
    }

    // 3. Перемещение объекта (мгновенное или плавное)
    void StartMovingObject(Step step)
    {
        if (step.targetObject != null)
        {
            // Можно сделать мгновенное перемещение
            // step.targetObject.transform.position = step.moveTargetPosition;

            // Или плавное перемещение (раскомментируйте нужный вариант)
            currentMovingStep = step;
            isMoving = true;

            Debug.Log($"Начато перемещение {step.targetObject.name} к позиции {step.moveTargetPosition}");
        }
    }

    void MoveObjectSmoothly(Step step)
    {
        // Плавное перемещение
        step.targetObject.transform.position = Vector3.MoveTowards(
            step.targetObject.transform.position,
            step.moveTargetPosition,
            step.moveSpeed * Time.deltaTime
        );

        // Проверяем, достигли ли цели
        if (Vector3.Distance(step.targetObject.transform.position, step.moveTargetPosition) < 0.01f)
        {
            step.targetObject.transform.position = step.moveTargetPosition;
            isMoving = false;
            currentMovingStep = null;
            Debug.Log($"Перемещение {step.targetObject.name} завершено");
        }
    }

    // 4. Изменение изображения
    void ChangeObjectImage(Step step)
    {
        if (step.newImageSprite == null)
        {
            Debug.LogWarning("Нет изображения для замены");
            return;
        }

        // Пытаемся найти компонент Image на объекте
        Image imageComponent = step.targetObject.GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = step.newImageSprite;
            Debug.Log($"Изображение на {step.targetObject.name} изменено");
            return;
        }

        // Пытаемся найти SpriteRenderer для 2D объектов
        SpriteRenderer spriteRenderer = step.targetObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = step.newImageSprite;
            Debug.Log($"Спрайт на {step.targetObject.name} изменен");
            return;
        }

        Debug.LogWarning($"На объекте {step.targetObject.name} нет компонента Image или SpriteRenderer");
    }
    // Вспомогательный метод для мгновенного перемещения (можно вызвать вместо плавного)
    void MoveObjectInstant(GameObject obj, Vector3 newPosition)
    {
        obj.transform.position = newPosition;
        Debug.Log($"Объект {obj.name} мгновенно перемещен в {newPosition}");
    }
}