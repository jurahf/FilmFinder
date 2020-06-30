declare @es_id int = 11; -- номер ЭС, которую нужно оставить (удалятся все, которые меньше)

delete from GoalStackSet
where Variable_Id in 
(select id from VariableSet
where ExpertSystemVariable_Variable_Id < @es_id)

delete from ConsultationRuleSet
where Rule_Id in 
(select id from RuleSet
where Result_Id in
(select id from FactSet
where Variable_Id in
(select id from VariableSet
where ExpertSystemVariable_Variable_Id < @es_id)))


delete from ConsultationSet
where CurrentRule_Id in 
(select id from RuleSet
where Result_Id in
(select id from FactSet
where Variable_Id in
(select id from VariableSet
where ExpertSystemVariable_Variable_Id < @es_id)))


delete from RuleSet
where Result_Id in
(select id from FactSet
where Variable_Id in
(select id from VariableSet
where ExpertSystemVariable_Variable_Id < @es_id))


delete from FactSet
where Variable_Id in
(select id from VariableSet
where ExpertSystemVariable_Variable_Id < @es_id)


delete from VariableSet
where ExpertSystemVariable_Variable_Id < @es_id

delete from ConsultationFactSet
where Fact_Id in
(select id from FactSet
where DomainValue_Id in
(select id from DomainValueSet
where DomainDomainValue_DomainValue_Id in
(select id from DomainSet
where ExpertSystemDomain_Domain_Id < @es_id)))


delete from RuleFactSet
where Rule_Id in
(select id from RuleSet
where ExpertSystemRule_Rule_Id < @es_id)


delete from SessionSet
where SessionConsultation_Session_Id in
(select id from ConsultationSet
where ExpertSystem_Id < @es_id)


delete from ConsultationSet
where ExpertSystem_Id < @es_id


delete from RuleSet
where ExpertSystemRule_Rule_Id < @es_id

delete from FactSet
where DomainValue_Id in
(select id from DomainValueSet
where DomainDomainValue_DomainValue_Id in
(select id from DomainSet
where ExpertSystemDomain_Domain_Id < @es_id))


delete from DomainValueSet
where DomainDomainValue_DomainValue_Id in
(select id from DomainSet
where ExpertSystemDomain_Domain_Id < @es_id)

delete from DomainSet
where ExpertSystemDomain_Domain_Id < @es_id


delete from ExpertSystemSet
where Id <> @es_id
