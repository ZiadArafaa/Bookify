var dataValues;
var dataKeys;
var options;
$(document).ready(function () {
    $.get({
        url: "/Dashboard/RentalPerDays",
        success: function (data) {
            dataValues = data.map(function (item) {
                return item.value;
            });
            dataKeys = data.map(function (item) {
                return item.key;
            });
            options = {
                chart: {
                    type: 'area'
                },
                series: [{
                    name: 'rentals',
                    data: dataValues
                }],
                xaxis: {
                    categories: dataKeys
                },
                yaxis: {
                    min: 0,
                    tickAmount: Math.max(...dataValues),
                }
            }

            var chart = new ApexCharts(document.querySelector("#chart"), options);
            chart.render();
        },
        error: function () {

        }
    });

    $.get({
        url: "/Dashboard/SubscriberByCity",
        success: function (data) {
            dataValues = data.map(function (item) {
                return item.value;
            });
            dataKeys = data.map(function (item) {
                return item.key;
            });
            options = {
                chart: {
                    type: 'donut'
                },
                series: dataValues,
                labels: dataKeys,
                plotOptions: {
                    pie: {
                        startAngle: 0,
                        endAngle: 360,
                        expandOnClick: true,
                        customScale: 1.1,
                        donut: {
                            labels: {
                                show: true,
                            },
                        },
                        
                    }
                }
            }

            var chart = new ApexCharts(document.querySelector("#chartSubscribers"), options);
            chart.render();
        },
        error: function () {

        }
    });
});