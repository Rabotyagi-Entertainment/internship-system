namespace internship_system.Common.Enums;

public enum DiaryState {
    Draft, // Заполняет шаблон
    OnMentorCheck, // На проверке ментора
    MentorApproved, // Одобрено ментором
    OnDeanCheck, // На проверке деканата
    DeanApproved, // Проверено деканатом
    OnCompanySignature, // на подписи компании
    OnDeanSignature, // На подписи деканата
    Done, // Сдан
}