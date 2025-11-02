<script>
    document.addEventListener("DOMContentLoaded", async function () {
    try {
        const response = await fetch("/Home/GetTrainerStats");
    const data = await response.json();

    // Переводимо дні у правильний порядок
    const days = ["Monday","Tuesday","Wednesday","Thursday","Friday","Saturday","Sunday"];
    const dayLabels = ["Пн","Вт","Ср","Чт","Пт","Сб","Нд"];

        // Визначаємо унікальних тренерів
        const trainers = [...new Set(data.map(d => d.trainer))];

        // Для кожного тренера формуємо дані по днях
        const datasets = trainers.map((trainer, i) => {
            const trainerData = days.map(day => {
                const record = data.find(d => d.trainer === trainer && d.day === day);
    return record ? record.clients : 0;
            });

    const colors = [
    "rgba(255,69,0,0.6)",
    "rgba(0,150,255,0.6)",
    "rgba(0,200,100,0.6)",
    "rgba(255,200,0,0.6)",
    "rgba(200,0,255,0.6)"
    ];

    return {
        label: trainer,
    data: trainerData,
    backgroundColor: colors[i % colors.length],
    borderColor: colors[i % colors.length],
    borderWidth: 2,
    tension: 0.4,
    fill: true
            };
        });

    new Chart(document.getElementById("trainerActivityChart"), {
        type: "line",
    data: {
        labels: dayLabels,
    datasets: datasets
            },
    options: {
        responsive: true,
    plugins: {
        legend: {labels: {color: "#fff" } },
    title: {
        display: true,
    text: "Кількість клієнтів за днями тижня",
    color: "#fff"
                    }
                },
    scales: {
        x: {ticks: {color: "#fff" }, grid: {color: "rgba(255,255,255,0.1)" } },
    y: {ticks: {color: "#fff" }, grid: {color: "rgba(255,255,255,0.1)" } }
                }
            }
        });
    } catch (err) {
        console.error("Помилка завантаження статистики:", err);
    }
});
</script>
