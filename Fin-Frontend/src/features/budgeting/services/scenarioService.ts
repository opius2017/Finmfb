import { Scenario, ScenarioComparison, WhatIfAnalysis, ImpactAnalysis } from '../types/scenario.types';

export class ScenarioService {
  private apiEndpoint = '/api/budgets/scenarios';

  async getScenarios(budgetId: string): Promise<Scenario[]> {
    const response = await fetch(`${this.apiEndpoint}?budgetId=${budgetId}`);
    if (!response.ok) throw new Error('Failed to fetch scenarios');
    return response.json();
  }

  async createScenario(scenario: Partial<Scenario>): Promise<Scenario> {
    const response = await fetch(this.apiEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(scenario),
    });
    if (!response.ok) throw new Error('Failed to create scenario');
    return response.json();
  }

  async updateScenario(scenarioId: string, scenario: Partial<Scenario>): Promise<Scenario> {
    const response = await fetch(`${this.apiEndpoint}/${scenarioId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(scenario),
    });
    if (!response.ok) throw new Error('Failed to update scenario');
    return response.json();
  }

  async compareScenarios(scenarioIds: string[]): Promise<ScenarioComparison> {
    const response = await fetch(`${this.apiEndpoint}/compare`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ scenarioIds }),
    });
    if (!response.ok) throw new Error('Failed to compare scenarios');
    return response.json();
  }

  async performWhatIfAnalysis(
    scenarioId: string,
    variable: string,
    testValues: number[]
  ): Promise<WhatIfAnalysis> {
    const response = await fetch(`${this.apiEndpoint}/${scenarioId}/what-if`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ variable, testValues }),
    });
    if (!response.ok) throw new Error('Failed to perform what-if analysis');
    return response.json();
  }

  async analyzeImpact(scenarioId: string): Promise<ImpactAnalysis> {
    const response = await fetch(`${this.apiEndpoint}/${scenarioId}/impact`);
    if (!response.ok) throw new Error('Failed to analyze impact');
    return response.json();
  }
}

export const scenarioService = new ScenarioService();
