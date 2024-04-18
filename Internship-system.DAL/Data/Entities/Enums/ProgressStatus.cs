using System.Runtime.Serialization;

namespace Internship_system.DAL.Data.Entities.Enums;

public enum ProgressStatus {
    SubmittedResume, // Подал резюме
    InSelectionProcess, // Прохожу отбор
    ReceivedOffer, // Получил оффер
    WasRefused, // Мне отказали
    AcceptedOffer // Принял оффер
}