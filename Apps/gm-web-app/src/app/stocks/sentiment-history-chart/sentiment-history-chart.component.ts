import { Component, Input, OnInit } from '@angular/core';
import { MatSelectChange } from '@angular/material/select';
import { ChartDataSets, ChartOptions, ChartType } from 'chart.js';
import { Color, Label } from 'ng2-charts';
import { StockRecommendation } from '../stock-recommendation';
import { StockRecommendationHistory } from '../stock-recommendation-history';

@Component({
  selector: 'gm-sentiment-history-chart',
  templateUrl: './sentiment-history-chart.component.html',
  styleUrls: ['./sentiment-history-chart.component.scss']
})
export class SentimentHistoryChartComponent implements OnInit {

  @Input() history: StockRecommendationHistory | undefined
  @Input() maxLength: number = 30;

  sentimentAxis: ChartDataSets[];
  labelAxis: Label[];
  chartOptions = {
    responsive: true
  };

  lineChartColors: Color[] = [
    {
      borderColor: '#1b603a',
      backgroundColor: 'rgba(196,210,83,0.25)',
    }
  ];
  lineChartLegend = true;
  lineChartPlugins = [];
  lineChartType: ChartType = 'line';
  selectedChart = 'sentiment';

  constructor() { 
    this.sentimentAxis = [];
    this.labelAxis = [];

  }

  ngOnInit(): void {
    this.setToSentimentChart();
  }

  updateChart(selection: MatSelectChange) {
    if(selection.value === 'confidence') {
      this.setToConfidenceChart();
    } else if(selection.value === 'price') {
      this.setToPriceChart();
    } else {
      this.setToSentimentChart();
    }
  }

  setToSentimentChart() {
    if(this.history) {
      const sortedR = this.history.historicalData.sort((n1,n2)=> n1.date.getUTCDate() - n2.date.getUTCDate());
      let data = [];
      let borders = [];
      let backgrounds = [];
      this.lineChartColors = [
        {
          borderColor: this.history.lastRecommendation.getHsl(25),
          backgroundColor: this.history.lastRecommendation.getHsl(90),
          pointBorderWidth: 2,
          pointRadius: 4
        }
      ];
      this.sentimentAxis = [];
      this.labelAxis = [];
      
      for(let i = 0; i < sortedR.length && this.maxLength; ++i) {
        borders.push(sortedR[i].getHsl(25));
        backgrounds.push(sortedR[i].getHsl(70));
        data.push(sortedR[i].sentiment);
        this.labelAxis.push((sortedR[i].date.getMonth() + 1).toString() + '/' + sortedR[i].date.getDate().toString());
      }

      this.lineChartColors[0].pointBackgroundColor = backgrounds;
      this.lineChartColors[0].pointBorderColor = borders;

      this.sentimentAxis.push({
        data: data, label: 'Sentiment'
      });
    }
  }

  setToConfidenceChart() {
    if(this.history) {
      const sortedR = this.history.historicalData.sort((n1,n2)=> n1.date.getUTCDate() - n2.date.getUTCDate());
      let data = [];
      let borders = [];
      let backgrounds = [];
      this.lineChartColors = [
        {
          borderColor: this.history.lastRecommendation.getHsl(25),
          backgroundColor: this.history.lastRecommendation.getHsl(90),
          pointBorderWidth: 2,
          pointRadius: 4
        }
      ];
      this.sentimentAxis = [];
      this.labelAxis = [];
      for(let i = 0; i < sortedR.length && this.maxLength; ++i) {
        borders.push(sortedR[i].getHsl(25));
        backgrounds.push(sortedR[i].getHsl(70));
        data.push(sortedR[i].confidence);
        this.labelAxis.push((sortedR[i].date.getMonth() + 1).toString() + '/' + sortedR[i].date.getDate().toString());
      }

      this.lineChartColors[0].pointBackgroundColor = backgrounds;
      this.lineChartColors[0].pointBorderColor = borders;

      this.sentimentAxis.push({
        data: data, label: 'Confidence'
      });
    }
  }

  setToPriceChart() {
    if(this.history) {
      const sortedR = this.history.historicalData.sort((n1,n2)=> n1.date.getUTCDate() - n2.date.getUTCDate());
      let s = [];
      let borders = [];
      let backgrounds = [];
      this.lineChartColors = [
        {
          borderColor: this.history.lastRecommendation.getHsl(25),
          backgroundColor: this.history.lastRecommendation.getHsl(90),
          pointBorderWidth: 2,
          pointRadius: 4
        }
      ];
      this.sentimentAxis = [];
      this.labelAxis = [];
      for(let i = 0; i < sortedR.length && this.maxLength; ++i) {
        borders.push(sortedR[i].getHsl(25));
        backgrounds.push(sortedR[i].getHsl(70));
        s.push(sortedR[i].lastData.close);
        this.labelAxis.push((sortedR[i].date.getMonth() + 1).toString() + '/' + sortedR[i].date.getDate().toString());
      }

      this.lineChartColors[0].pointBackgroundColor = backgrounds;
      this.lineChartColors[0].pointBorderColor = borders;
      
      this.sentimentAxis.push({
        data: s, label: 'Price'
      });
    }
  }
}
