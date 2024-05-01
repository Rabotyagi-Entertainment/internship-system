namespace internship_system.Common.Enums;

public enum ProgressStatus {
    Default, // Студент зафиксировал в системе желание проходить стажировку в определённой компании, но пока не сделал ничего кроме
    SubmittedResume, // Подал резюме
    InSelectionProcess, // Прохожу отбор
    ReceivedOffer, // Получил оффер
    WasRefused, // Мне отказали
    AcceptedOffer // Принял оффер
}